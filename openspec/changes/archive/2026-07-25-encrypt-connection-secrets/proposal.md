## Why

`Connection.ConnectionProperties` is stored as plaintext JSON despite its XML doc claiming
encryption, and `GET /connections/{id}` returns the raw entity unfiltered — so even once secrets
are encrypted at rest, any authorized caller currently gets them back in plaintext over the API.
This blocks the "more secure than Redash" goal (Roadmap §4.1, ChangeSpecs P0-1) and is the
highest-severity item in the roadmap.

## What Changes

- Add a new `Connection.SecretProperties` column, separate from the existing (unchanged, still
  plaintext) `Connection.ConnectionProperties`. Secret-flagged parameter values live only in the
  new column, encrypted with AES-GCM; non-secret values are unaffected.
- Add `ISecretProtector` (+ AES-GCM default implementation) with a key sourced from standard
  ASP.NET Core configuration (env var recommended).
- Add a shared "which parameter keys are secret" resolver based on reflecting
  `[ConnectorParameter(..., ConnectionPropertyType.ProtectedText)]` on the connector's CLR type —
  used by connection create, connection update, and connector execution.
- `QueryExecutor.GetConnector` explicitly decrypts `SecretProperties` and merges it with
  `ConnectionProperties` into one flat dictionary before constructing the connector — connector
  code (`ConnectorBase` and subclasses) is unchanged and stays unaware encryption exists.
- **BREAKING**: `GET /connections/{id}` no longer returns the raw `Connection` entity. It returns
  a new response shape where secret values are presence-only (`{"Password": true}`) — never a
  value, never ciphertext, never a placeholder string like `"********"`.
- **BREAKING**: `POST /connections` now splits incoming `ConnectionProperties` server-side into
  plaintext vs. encrypted storage based on the secret-key resolver; the request shape is
  unchanged, but stored representation is not a straight passthrough anymore.
- Add a new `PUT /connections/{id}` endpoint (none exists today). Non-secret properties are
  overwritten wholesale; a secret key omitted from the request body leaves the existing encrypted
  value untouched (no sentinel matching — omission is the only "unchanged" signal).
- Fix `ConnectionsController.Get` authorizing against `GridOperations.Create` instead of a read
  operation (copy-paste bug found while touching this controller).
- `ConnectionPropertyEditor.razor` leaves `ProtectedText` fields blank when editing an existing
  connection, and only sends a value the user actually typed.
- **BREAKING**: Squash all existing EF migrations (Postgres + Sqlite) into a single fresh
  `Initial` migration that includes `SecretProperties` from the start. No data migration — the app
  is not deployed anywhere, so there is no plaintext-secret data to carry forward.

## Capabilities

### New Capabilities
- `connection-secret-management`: encryption at rest for connection secret parameters, redacted
  (presence-only) exposure over the API, create/update/read/execute handling of secret vs.
  non-secret connection properties.

### Modified Capabilities
_(none — no existing specs in this repo yet; connection CRUD behavior is captured as part of the
new capability above since it did not previously have a spec.)_

## Impact

- **Schema**: new `Connection.SecretProperties` column; squashed migration baseline for
  `TheGrid.Postgres` and `TheGrid.Sqlite`.
- **API**: `GET /connections/{id}` response shape changes; new `PUT /connections/{id}` endpoint;
  `POST /connections` storage behavior changes (request/response shape unchanged).
- **Services**: new `ISecretProtector` abstraction + AES-GCM implementation; new shared
  secret-parameter-key resolver; `QueryExecutor.GetConnector` updated to merge decrypted secrets.
- **Config**: new configuration key for the encryption key (`SystemOptions` or a new options
  class), documented as env-var-first.
- **Client**: `ConnectionPropertyEditor.razor` password-field UX change.
- **Docs**: `docs/AddingMigrations.md` gains Sqlite instructions (currently Postgres-only).
- **Out of scope**: P1-1's real `Key`/`IsSecret` attribute model, P0-2/P0-3/P0-4/P0-5, and all
  other Phase 1 SDK items — this change only covers secret encryption + the connection CRUD
  surface needed to support it.
