## Why

`docs/Creating-Connectors.md` documents a contract that no longer exists: `QueryRunner`,
`QueryRunnerBase`, `QueryRunnerAttribute`, `QueryRunnerParameterAttribute`, `RunQueryAsync`, and a
buffered `QueryResult` with `List<string> Columns`/`List<Dictionary<string,object>> Rows`. The real
code uses `Connector`/`ConnectorBase`/`[Connector]`/`[ConnectorParameter]`/`GetDataAsync`, and
`GetDataAsync` is now a streaming `IAsyncEnumerable<ConnectorRow>` (post the
`streaming-query-execution-limits` change), not a buffered return. Anyone — including a future
contributor or a coding assistant — following this doc today cannot produce a working connector; every
type and method name it references fails to compile.

## What Changes

- Rewrite `docs/Creating-Connectors.md` end-to-end to match the current, verified contract (per
  `docs/architecture/CurrentState.md`'s Connector SDK table):
  - `[Connector(name, EditorLanguage, IconFileName)]` on the class.
  - Inherit `ConnectorBase`, constructor takes `Dictionary<string, string> connectorParameters`.
  - `[ConnectorParameter(name, ConnectionPropertyType, RenderOrder, HelpText, Required)]`, one per
    parameter, `AllowMultiple = true`. Note the current trap: parameters are keyed by the attribute's
    display `Name` string, not a separate machine key — document this as current behavior, since it's
    still true until `P1-1` lands.
  - `public override async IAsyncEnumerable<ConnectorRow> GetDataAsync(string query, Dictionary<string,
    object?>? queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)`
    — a streaming method using `yield return`, not a method that builds and returns a buffered result
    object.
  - `ConnectorRow(IReadOnlyDictionary<string, QueryResultColumn> Columns, IReadOnlyDictionary<string,
    object?> Data)` — one row per yield; `Columns` built once from the first row and reused.
  - Capability interfaces `ISchemaDiscovery`, `IConnectionTest`, `IPermissionTest` — document their
    actual current method signatures (`GetSchemaAsync`, `TestConnectionAsync` returning bare `bool`,
    `HasWritePermissionAsync` returning bare `bool`).
  - Use `PostgreSqlConnector` (`source/TheGrid.Connectors/PostgreSqlConnector.cs`) as the worked
    example throughout, referenced directly rather than an invented "MyDatabaseRunner" class, so the
    doc can never drift from a real, compiling connector again.
- No code changes — this is a documentation-only change.

## Capabilities

### New Capabilities
- `connector-authoring-guide`: defines what `docs/Creating-Connectors.md` must accurately describe
  about the current connector contract, so the doc's accuracy is itself a checkable requirement (and
  future connector-contract changes have an explicit doc-update obligation to point at).

### Modified Capabilities
(none — no runtime behavior changes)

## Impact

- `docs/Creating-Connectors.md` — full rewrite.
- No source code changes.
- Future connector-contract changes (P1-1 Key/IsSecret, P1-2 Abstractions split, P1-3
  ConnectorFactory, P1-5 capability-result enrichment) will each need a follow-up doc update — this
  change's spec gives those later changes a concrete "doc must reflect X" requirement to extend rather
  than starting from scratch.
