## Context

Today `ConnectorParameterAttribute`'s only identifying string is `Name` — both the UI label and the
literal runtime dictionary key. Every layer touches it: the attribute itself, `ConnectionProperty`
(the Mapster-adapted DTO), `ConnectorBase.ValidateParameters`, `ConnectorExtensions.GetSecretParameterKeys`,
`ConnectionsController.Post`/`Put` (splitting incoming properties into secret/non-secret), each
connector's own parameter lookups (`PostgreSqlConnector.GetConnection`, `TestConnector`), and the client
(`ConnectionPropertyEditor.razor.cs`, `CreateConnection.razor.cs`, `EditConnection.razor.cs`). All of
these were read directly (not assumed) while grounding this design — full list in Impact below.

One useful discovery: `ConnectionsController.Post`/`Put` themselves need **no code change**. They
already operate generically on whatever string keys are present in `GetSecretParameterKeys()`'s
returned set and the incoming request dictionary — they don't know or care whether those strings are
display names or machine keys. Only what the *strings mean* changes, not the controller logic. This
significantly shrinks the actual blast radius versus what it might look like from the roadmap doc alone.

## Goals / Non-Goals

**Goals:**
- A parameter has a `Key` (stable, machine-oriented, immutable in practice) distinct from `Name`
  (display label, freely changeable).
- `IsSecret` is an explicit, independently-settable fact, while `ProtectedText` still implies it
  automatically so existing connectors don't need to change to remain correctly encrypted.
- Every runtime dictionary lookup (`ConnectorParameters`, `ConnectionProperties`, `SecretProperties`)
  switches from `Name`-keyed to `Key`-keyed.
- `PostgreSqlConnector` and `TestConnector` get real `Key` values and keep working end-to-end.

**Non-Goals (explicitly deferred, see proposal.md):**
- `Group`, `DefaultValue`, `Placeholder`, `VisibleWhen`, `Validation` — no current consumer, not built.
- Any automated migration of already-stored `ConnectionProperties`/`SecretProperties` data — see
  proposal.md's "Data migration approach"; existing local connections are recreated, not migrated.
- No change to `IConnectorFactory`/discovery/Abstractions-split — those are `P1-2`/`P1-3`, sequenced
  after this change.

## Decisions

**`ConnectorParameterAttribute` constructor becomes `(string key, string name, ConnectionPropertyType
propertyType)`, with `key` first.**
Both are now required, positional, constructor-enforced — matching how `Name` is already
constructor-enforced today (not a settable-after-construction property), and putting `key` first
signals it's the primary identity, `name` the secondary/display one.

**`Key` validation is stricter than `Name`'s.**
`Name`'s existing constructor check allows letters, digits, spaces, underscores, hyphens (it's a label).
`Key` must be a valid dictionary-safe identifier: starts with a letter, then letters/digits/underscores
only (`^[A-Za-z][A-Za-z0-9_]*$`), enforced the same way (`ArgumentException` in the constructor) `Name`'s
check already is.

**`CommonConnectionParameters` constants change meaning from "common display names" to "common parameter
keys."**
Today `CommonConnectionParameters.Password == "Password"` and that same constant is used both as the
attribute's (only) identifying string and as the runtime lookup key. After this change, the runtime
lookup must use `Key`, so the constants become key-shaped (`ConnectionString = "connectionString"`,
`DatabaseName = "databaseName"`, `Username = "username"`, `Password = "password"`, `PortNumber =
"portNumber"`, `Database = "database"`). The display `Name` argument becomes a plain literal string
written directly at each `[ConnectorParameter]` call site (`"Connection String"`, `"Database Name"`,
etc.) — display labels aren't meant to be a shared/reused constant the way keys are (two connectors
could reasonably word the "same" concept's label differently while still sharing a key convention).

**`IsSecret` is a plain, independently-settable bool (default `false`); "effective secrecy" is computed
where it's checked, not cached as a third property.**
Adding a computed `IsEffectivelySecret` property (or similar) to hold `IsSecret ||
Type == ConnectionPropertyType.ProtectedText` was considered, but there's exactly one place that needs
this today (`ConnectorExtensions.GetSecretParameterKeys`) — introducing a named abstraction for a single
call site is the premature-abstraction pattern this project explicitly avoids. If a second consumer
appears later, promoting the OR-expression to a shared extension method is a small, easy follow-up.

**Client-side: rename the `(string Name, string? Value)` tuple element to `(string Key, string?
Value)` everywhere it appears** (`ConnectionPropertyEditor.razor.cs`'s `ValueChanged` callback type,
`CreateConnection.razor.cs`/`EditConnection.razor.cs`'s `ParameterValueChanged` handlers), and switch
every dictionary read/write keyed off it (`_input.ConnectionProperties`, `_input.SecretProperties`,
`GetInitialValue`'s lookup, the secret/non-secret routing check in `EditConnection`'s
`ParameterValueChanged`) to use `.Key` instead of `.Name`. `EditConnection.ParameterValueChanged`'s
existing secret-routing check (`_selectedConnector?.Parameters.FirstOrDefault(p => p.Name ==
x.Name)?.Type == ProtectedText`) should also switch its match to `p.Key == x.Key` and its secrecy test
to `p.IsSecret || p.Type == ConnectionPropertyType.ProtectedText`, for consistency with the
server-side resolver.

## Risks / Trade-offs

- **[Risk]** Missing a `Name`→`Key` conversion at one of the several call sites leaves a silent runtime
  key mismatch (a lookup that always misses, e.g. a parameter that's always seen as "not set") rather
  than a compile error, since both sides are just `string`.
  **[Mitigation]** tasks.md enumerates every call site found during grounding (both server and client);
  the verification step re-runs the full test suite plus a manual create/edit-connection pass through
  the running app (via `docker-compose up`, per project convention) exercising `PostgreSqlConnector`
  end-to-end, not just unit tests in isolation.
- **[Risk]** Existing local dev connections silently "lose" their stored values (old `Name`-keyed
  dictionary entries no longer match new `Key`-based lookups) rather than erroring loudly.
  **[Mitigation]** Accepted per proposal.md's Data migration approach — flagged explicitly here again
  since it's the one change a user of this app will actually notice. Recreating a local connection takes
  under a minute.
- **[Risk]** `ConnectorDiscoveryService`'s unconfigured `Mapster.Adapt<ConnectionProperty>()` call
  (attribute → DTO) might not pick up the new `Key`/`IsSecret` properties automatically if Mapster's
  convention-based mapping doesn't match property names for some reason.
  **[Mitigation]** Verify directly during implementation (add/extend a unit test asserting a mapped
  `ConnectionProperty` has the same `Key`/`IsSecret` values as the source attribute) rather than assuming
  by analogy to the `fix-type-mapper` change's enum case — property-to-property mapping and enum-to-enum
  mapping are different Mapster code paths.

## Migration Plan

No EF migration (see proposal.md — the column shape is unchanged, only which strings are valid keys).
Application-level "migration" is: recreate any pre-existing local connections after this change is
deployed to a given environment. No automated tooling for this is built, per the Data migration approach
in proposal.md.

## Open Questions

None — scope, key format, and every affected call site were confirmed by reading the current code
directly during design.
