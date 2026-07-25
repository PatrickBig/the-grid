## Context

`Connection.ConnectionProperties` (`Dictionary<string,string?>`) is stored via
`JsonColumnConverter<T>` as plain JSON — no encryption despite the XML doc comment claiming
otherwise. `JsonColumnConverter<T>` is shared with `QueryResultRow.Data`, `Connector.Parameters`,
and `TableVisualization.Columns`, so it cannot be modified to add encryption without affecting
unrelated columns.

`GET /connections/{id}` currently does `return Ok(connection)` — the raw entity, secrets included
— so the live leak today is over the API, not just at rest. There is no `PUT`/update endpoint for
connections at all.

Only one shipped connector parameter is flagged `ConnectionPropertyType.ProtectedText` today
(`PostgreSqlConnector.Password`), so the blast radius for this change is small, but the mechanism
must generalize to future connectors and parameters.

This app is not deployed anywhere; the database can be destroyed and recreated freely, so there is
no production data or backward-compatibility constraint to design around.

## Goals / Non-Goals

**Goals:**
- Encrypt secret-flagged connection parameter values at rest, distinctly from non-secret values.
- Ensure no API response ever returns a secret value or its ciphertext — presence only.
- Support creating and updating connections, including partial secret updates, without a
  round-trip overwrite hazard.
- Let connector execution receive a plain, decrypted parameter dictionary exactly as it does
  today, with no encryption-awareness added to connector code.
- Resolve "which parameter keys are secret" from the connector's own attribute metadata, so it
  can't drift from what the connector actually declares.

**Non-Goals:**
- P1-1's stable `Key` + first-class `IsSecret` attribute model — this change uses
  `ConnectionPropertyType.ProtectedText` as the interim secret marker, per the roadmap's own
  sequencing note.
- Query execution limits, batched inserts, type-mapper fixes, connector doc rewrite (P0-2 through
  P0-5) — unrelated to secret handling.
- `IConnectorFactory`, streaming, plugin/multi-assembly discovery, `Abstractions` split (P1-2
  through P1-6).
- Key rotation tooling/automation. The storage format supports future rotation (key id travels
  with each ciphertext value), but building a rotation job is out of scope here.
- Migrating existing plaintext secret data. None needs to survive — see Migration Plan.
- A KMS/Vault-backed `ISecretProtector` implementation. Only the config-sourced AES-GCM default is
  built now; the interface is the seam for that later.

## Decisions

**1. Separate `SecretProperties` column, not in-place encryption of `ConnectionProperties`
values.**
Keeping one dictionary and encrypting only the values whose keys are secret would require every
read site (API responses, `QueryExecutor`, any future feature that touches connection properties)
to re-derive "is this key secret" before it can safely decide whether to decrypt/redact a given
value. A second column makes the two kinds of data structurally distinct: `ConnectionProperties`
is always safe to return as-is, `SecretProperties` never is. The secret/non-secret split is
decided once, at write time, not re-derived at every read.
*Alternative considered*: encrypt secret values in place within the existing dictionary. Rejected
— pushes classification logic to every call site and increases the chance a future change forgets
to check before serializing.

**2. Explicit orchestration in application code, not an EF `SavingChanges`/materialization
interceptor.**
Connector construction (`QueryExecutor.GetConnector`) must produce a plain, decrypted parameter
dictionary through a visible line of code, not through a side effect a reader of that method can't
see. An interceptor would also need mid-save access to the connector's CLR type to know which keys
are secret, which is awkward to plumb through EF's interceptor API.
*Alternative considered*: `DbContext` `SavingChanges`/materialization interceptors performing
transparent encrypt/decrypt. Rejected per explicit requirement that connector execution must
receive its secrets with no hidden magic.

**3. Secret-key resolution via reflection over `[ConnectorParameter(..., ProtectedText)]`, not a
DB round-trip to the cached `Connector.Parameters` row.**
`ConnectorDiscoveryService` already derives `Connector.Parameters` from these same attributes by
reflecting the connector type. Reflecting directly for secret-key resolution reuses the actual
source of truth instead of a persisted cache that could drift from the type if discovery hasn't
re-run. This becomes a small shared helper used by connection create, connection update, and
`QueryExecutor.GetConnector`.

**4. Presence-only redaction on read (`{"Password": true}`), not a sentinel string
(`"********"`).**
A sentinel value that looks like real (masked) data invites a common client bug: load the edit
form, user changes one field, resubmit the whole object — silently overwriting the real secret
with the literal sentinel text unless the server special-cases that exact string. Presence-only
output contains no string that could pass for a value, so there's nothing to accidentally send
back. This does put a small contract on the client: secret-type fields render blank on edit and
are only included in an update request if the user actually typed something.

**5. Update semantics: omission means "leave unchanged," not sentinel matching.**
Consistent with (4) — the update DTO's `SecretProperties` only contains keys the client is
actively setting. No key present means no change; there is no reserved "unchanged" value to match
against.

**6. AES-GCM with key id + nonce packed into the stored string per value, not a side table.**
`"v1:<keyId>:<base64(nonce ++ ciphertext ++ tag)>"` per value keeps `SecretProperties` a plain
`Dictionary<string,string?>` that reuses the existing `JsonColumnConverter<T>` unmodified — no
new entity, no join, no migration shape beyond one new column. The key id is carried so a future
rotation job can identify which values were encrypted under an old key without needing separate
metadata storage.

**7. Encryption key sourced through standard ASP.NET Core configuration (`IOptions`-bound), env
var as the recommended pattern.**
Config already flows through env vars, `appsettings.json`, and args uniformly via `IOptions`; no
new configuration mechanism is needed. A pluggable `ISecretProtector` interface leaves room for a
KMS/Vault-backed implementation later without changing callers.

**8. Squash EF migration history into a fresh `Initial` migration per provider, fold into this
change rather than a separate one.**
Nothing is deployed anywhere, so there's no environment carrying forward the ~2 years of existing
migration history, and no data migration is needed for the new column. Since this is the first
change to touch schema, squashing now (rather than adding one more incremental migration on top of
stale history) avoids doing the reset later at higher cost.

## Risks / Trade-offs

- **[Risk] Losing the configured encryption key makes all stored secrets permanently
  unrecoverable.** → Mitigation: document prominently in setup docs; this is an accepted trade-off
  of application-managed key material, consistent with the roadmap's own framing.
- **[Risk] AES-GCM nonce reuse under the same key is catastrophic for confidentiality.** →
  Mitigation: the protector implementation generates a fresh random 12-byte nonce per `Encrypt`
  call; callers never supply or reuse a nonce.
- **[Risk] A newly-flagged `ProtectedText` parameter on an existing connector doesn't retroactively
  re-encrypt values stored before the flag was added.** → Not applicable today (no data survives
  this change), but worth a code comment for future connector authors; full handling is part of
  P1-1's richer metadata model, not this change.
- **[Risk] Squashed migration history is a breaking operation for any environment that already
  applied old migrations.** → Mitigation: acceptable because nothing is deployed; explicitly called
  out here so it isn't applied against a real environment by mistake later.
- **[Risk] The new `PUT` endpoint expands authorization surface.** → Mitigation: reuse the existing
  `ConnectionAuthorizationHandler`/`GridOperations` resource-authorization pattern already used by
  `POST`/`GET`, rather than inventing new checks.

## Migration Plan

1. Delete all files under `TheGrid.Postgres/Migrations` and `TheGrid.Sqlite/Migrations`.
2. Update the EF model (`Connection.SecretProperties`, `DbContext` configuration).
3. Regenerate a single `Initial` migration per provider from the updated model.
4. No data backfill — no existing data needs to survive.
5. Rollback: since nothing is deployed, rollback is reverting the commit/branch; no runtime
   rollback procedure is needed.

## Open Questions

- Exact home for `ISecretProtector` and its options class — `TheGrid.Services` (consistent with
  other manager/service abstractions) vs. `TheGrid.Data` (where `JsonColumnConverter` lives).
  Leaning `TheGrid.Services`; confirm during implementation.
- Exact configuration key name/shape for the encryption key (single base64-encoded key is
  sufficient for this change; multi-key rotation support is deferred).
- Full field list for the new connection read/update DTOs beyond `ConnectionProperties` /
  `SecretProperties` (e.g. whether `Name`, `ConnectorId`, `OrganizationId` are echoed back) — an
  implementation-time detail, not a behavioral requirement.
