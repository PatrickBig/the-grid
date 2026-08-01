## Why

Every connector parameter today is identified only by its display `Name` (e.g. `"Connection String"`,
`"Password"`) — that same string is simultaneously the UI label, the dictionary key
`ConnectionProperties`/`SecretProperties`/`ConnectorParameters` are stored and looked up by, and the
key `ConnectorExtensions.GetSecretParameterKeys()` uses to decide what gets encrypted. This means
renaming a label is a silent breaking data change, there's no way to localize or reword a label without
losing stored values, and "is this secret" is inferred solely from
`ConnectionPropertyType.ProtectedText` rather than being an explicit, independently-settable fact. This
is `docs/roadmap/ChangeSpecs.md`'s P1-1, flagged as the linchpin Phase 1 item everything else in the
connector SDK rework depends on.

## What Changes

- **BREAKING**: `ConnectorParameterAttribute`'s constructor gains a required `key` parameter ahead of
  the existing `name`: `ConnectorParameterAttribute(string key, string name, ConnectionPropertyType
  propertyType)`. `Key` is validated as a machine identifier (letters/digits/underscore only, must
  start with a letter) — stricter than `Name`'s existing letters/digits/spaces/underscore/hyphen rule,
  since `Key` is meant to be dictionary-safe and stable, `Name` stays free-form for display.
- **BREAKING**: `ConnectionProperty` (`TheGrid.Shared.Models`) gains a `Key` property (mapped from the
  attribute via the existing unconfigured Mapster `.Adapt<ConnectionProperty>()` call in
  `ConnectorDiscoveryService`).
- **BREAKING**: `ConnectorParameterAttribute` and `ConnectionProperty` gain an explicit `IsSecret` bool
  (default `false`). Effective secrecy is `IsSecret || Type == ConnectionPropertyType.ProtectedText` —
  `ProtectedText` continues to imply secret behavior automatically, but `IsSecret` can also be set
  independently for a future non-`ProtectedText` field that still needs encryption-at-rest.
- **BREAKING**: Every place that currently uses a parameter's `Name` as a runtime dictionary key
  switches to `Key`: `ConnectorBase.ValidateParameters`, `ConnectorExtensions.GetSecretParameterKeys`,
  `ConnectionsController.Post`/`Put` (splitting request properties into
  `ConnectionProperties`/`SecretProperties`), `PostgreSqlConnector.GetConnection` (and its
  `CommonConnectionParameters` constants, which become `Key` values), `TestConnector`'s
  `"NumberOfRows"` lookup, and the client (`ConnectionPropertyEditor.razor.cs`'s `ValueChanged` tuple,
  `CreateConnection.razor.cs`/`EditConnection.razor.cs`'s `ParameterValueChanged`).
- `PostgreSqlConnector` and `TestConnector`'s attributes get real `Key` values (e.g. `connectionString`,
  `databaseName`, `username`, `password`, `numberOfRows`) alongside their unchanged display `Name`s.

## Explicitly out of scope (scoped down from the original roadmap item)

`docs/roadmap/ChangeSpecs.md`'s P1-1 lists `Group`, `DefaultValue`, `Placeholder`, `VisibleWhen`, and a
`Validation` sub-object as *optional* additions. None of these have any consumer anywhere in the
codebase today (no UI grouping, no conditional field visibility, no client-side validation beyond the
`Required` bool that already exists) — adding them now would be speculative metadata with nothing to
exercise it. Per this project's stated preference against premature abstraction, this change adds only
`Key` and `IsSecret`, the two pieces of metadata that unblock the encryption/redaction the app already
does. `Group`/`DefaultValue`/etc. can be a later, separately-justified change once a concrete UI need
exists for them.

## Data migration approach

`docs/roadmap/ChangeSpecs.md`'s original P1-1 acceptance criteria call for a data migration mapping
existing display-name-keyed `ConnectionProperties`/`SecretProperties` to the new `Key`-keyed shape. That
requirement was written before the 2026-07-25 note (top of `ChangeSpecs.md`) established this app has no
production deployment or real customer data — the migration-safety concern doesn't apply. **No EF
migration is needed at all** (the column stays `Dictionary<string,string?>`; only which strings are
valid keys changes), and no runtime data-remapping script is written either. Any connection created
before this change (e.g. a local dev Postgres connection) will have its stored dictionary keyed by the
old display-name strings, which will no longer match the new `Key`-based lookups — **existing
connections must be deleted and recreated** after this change lands. This is called out explicitly so
it isn't a silent surprise.

## Capabilities

### New Capabilities
- `connector-parameter-identity`: defines the `Key`/`Name`/`IsSecret` contract for connector parameter
  metadata — what identifies a parameter at rest and in code vs. what's shown to a user, and how
  secrecy is determined.

### Modified Capabilities
- `connection-secret-management`: the existing spec's secret-key resolution
  (`GetSecretParameterKeys`) is currently defined in terms of `ConnectionPropertyType.ProtectedText`
  only and returns `Name`-keyed strings; this changes it to `Key`-keyed strings determined by
  `IsSecret || ProtectedText`.

## Impact

- `TheGrid.Connectors/Attributes/ConnectorParameterAttribute.cs` — new `Key` ctor param, new `IsSecret`
  property.
- `TheGrid.Shared/Models/ConnectionProperty.cs` — new `Key`, `IsSecret` properties.
- `TheGrid.Connectors/ConnectorBase.cs` — `ValidateParameters` keys off `Key`.
- `TheGrid.Connectors/Extensions/ConnectorExtensions.cs` — `GetSecretParameterKeys` keys off `Key`,
  checks `IsSecret || ProtectedText`.
- `TheGrid.Connectors/PostgreSqlConnector.cs`, `TestConnector.cs`, `CommonConnectionParameters.cs` —
  add `Key` values to every `[ConnectorParameter]`; `GetConnection`/parameter lookups switch to `Key`
  constants.
- `TheGrid.Server/Controllers/ConnectionsController.cs` — `Post`/`Put` split properties by `Key`.
- `TheGrid.Client/Shared/ConnectionManagement/ConnectionPropertyEditor.razor(.cs)`,
  `Pages/ConnectionManagement/CreateConnection.razor(.cs)`, `EditConnection.razor(.cs)` — emit/consume
  `Key` instead of `Name` as the dictionary key.
- `TheGrid.Services/ConnectorDiscoveryService.cs` — no code change expected (unconfigured Mapster
  `.Adapt<ConnectionProperty>()` should pick up the new attribute properties by name automatically,
  same pattern as the `fix-type-mapper` change relied on for enum mapping); verify during
  implementation.
- No EF migration. Existing local connections need manual recreation post-deploy (see above).
