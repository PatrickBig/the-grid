## Why

Phase 2 of the roadmap calls for proving the connector SDK's breadth by building differently-shaped
connectors, not just a second relational database. MongoDB is deliberately chosen because it stresses
three assumptions baked into the SDK from its Postgres-only history: what `Query.Command` means for a
document store, how schema discovery works against a schemaless data source, and whether the
flat-row/flat-column `ConnectorRow` shape holds up against nested documents. Building this connector
either validates those assumptions or surfaces where the SDK needs to flex.

## What Changes

- Add a `MongoDbConnector` to `TheGrid.Connectors` implementing `ConnectorBase`, `ISchemaDiscovery`,
  and `IConnectionTest` (write-access probing via `IWriteAccessProbe` is left out of this change — see
  Impact).
- Define `Query.Command`'s meaning for this connector as a JSON object with `collection`, `query`,
  `aggregate`, `projection`, `sort`, `skip`, `limit`, `count`, `db`, and `allowDiskUse` keys, modeled on
  Redash's MongoDB query contract with one deliberate deviation: `sort` uses native MongoDB sort-object
  syntax (`{"field": 1}`) instead of Redash's ordered-array-of-objects dialect, since
  `System.Text.Json` preserves object property order and the native form is simpler and more familiar.
- Add an optional (not required) `Database` connector parameter as a default database, overridable
  per-query via the command JSON's `db` key — reflecting that Mongo connections are naturally
  cluster/deployment-scoped rather than single-database like Postgres.
- Implement `GetSchemaAsync` via MongoDB's native `{ $sample: { size: N } }` aggregation stage per
  collection, with `N` exposed as a tunable `SchemaSampleSize` connector parameter. Per-field type
  inference across the sample is recorded via `DatabaseObjectColumn.TypeName` (BSON type, or `"Mixed"`
  when it varies) and `Attributes` (`ObservedTypes` when mixed, `Presence` as an observed-fraction
  string). No oldest/newest/"middle" sampling — a single random sample is the deliberate MVP.
  Multi-collection metadata (schema caching, refresh scheduling, an explorer UI) is explicitly excluded
  — see Impact.
- Map each MongoDB document to one `ConnectorRow`; nested subdocuments/arrays are kept as a single raw
  cell value (not flattened into dotted-path columns) and tagged with `QueryResultColumnType.Json`.
- **BREAKING (client-visible only, not an API contract break)**: `TheGrid.Client`'s `Table` visualization
  gains rendering support for `QueryResultColumnType.Json` cells — collapsed by default (e.g. `{...}`),
  expandable into a tree view on click. Today such cells silently render as raw inline JSON text; this
  changes that rendering for any connector's `Json`-typed columns, not just Mongo's.

## Capabilities

### New Capabilities
- `mongodb-connector`: MongoDB connector's command JSON contract (find/aggregate mode selection, native
  sort syntax, per-query database override), connection-level default-database parameter, `$sample`-based
  schema discovery with tunable sample size and per-field type/presence inference, and document-to-row
  mapping with nested values preserved as `Json`-typed cells.
- `table-json-rendering`: `Table` visualization behavior for columns typed `QueryResultColumnType.Json`
  — collapsed-by-default rendering with click-to-expand tree view. Connector-agnostic; Mongo is the
  first real producer of `Json`-typed columns; the `Json` enum value itself already exists
  (`query-result-type-mapping`) and is unchanged by this capability.

### Modified Capabilities
(none — this connector conforms to existing capabilities such as `connector-streaming`,
`connector-capability-results`, `connector-parameter-identity`, `connector-instantiation`, and
`connection-secret-management` without changing their requirements)

## Impact

- **New code**: `source/TheGrid.Connectors/MongoDbConnector.cs`, plus a MongoDB driver package
  reference added to `TheGrid.Connectors.csproj`.
- **Modified code**: `source/TheGrid.Client/Shared/Visualizations/Table.razor` and `Table.razor.cs`
  (add a `Json`-typed rendering branch; `GetTypeForColumnType` currently has no `Json` case and falls
  through to `typeof(string)`, and the `<Template>` markup has no collapse/expand logic).
- **Explicitly out of scope** (deferred to future, separately-proposed changes):
  - Schema **caching**, a scheduled refresh job, "last refreshed at" UI, manual refresh trigger, an
    explorer view, and multi-user refresh race handling. `ISchemaDiscovery.GetSchemaAsync()` has zero
    callers in `TheGrid.Server`/`TheGrid.Client` today for any connector (including Postgres) — this is
    a pre-existing, cross-cutting gap, not something this connector needs to fix, and it doesn't touch
    the query execution path (`QueryExecutor` never calls `ISchemaDiscovery`).
  - A Redash-`$humanTime`-style relative-date macro/injection system for query literals. Worth
    revisiting as this connector's own Extended-JSON preprocessing extension later, but one connector
    wanting it isn't enough evidence to generalize into a shared SDK abstraction.
  - User-supplied `{{param}}`-style query parameters. This is a pre-existing, connector-agnostic gap
    (`queryParameters` is hardcoded `null` through the whole `QueryExecutor` → `QueryRefreshManager` →
    Hangfire → `RefreshQueryResultsRequest` call chain) tracked separately from connector work.
  - `IWriteAccessProbe` (write-access probing) for this connector — Mongo's permission model
    (role-based, per-database/per-collection privileges via `usersInfo`/`connectionStatus`) doesn't map
    onto a single cheap query the way Postgres's `has_table_privilege` does; left for a follow-up rather
    than under-implementing it here.
