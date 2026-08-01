## 1. Setup

- [x] 1.1 Add the official MongoDB .NET driver package (`MongoDB.Driver`) reference to
      `TheGrid.Connectors.csproj`.
- [x] 1.2 Scaffold `source/TheGrid.Connectors/MongoDbConnector.cs`: `[Connector("MongoDB", ...)]`
      class inheriting `ConnectorBase(ConnectorContext context)`, with `[ConnectorParameter]`s for
      connection string (or host/port), `Database` (optional, not required), credentials as
      applicable, and `SchemaSampleSize` (numeric, default e.g. 100).

## 2. Command parsing

- [x] 2.1 Define an internal model (or parse directly with `System.Text.Json`) for the command JSON
      shape: `collection`, `query`, `aggregate`, `projection`, `sort`, `skip`, `limit`, `count`, `db`,
      `allowDiskUse`.
- [x] 2.2 Implement mode selection: presence of `aggregate` → `.Aggregate()`; otherwise → `.Find()`
      using `query`/`projection`/`sort`/`skip`/`limit`.
- [x] 2.3 Implement `sort` using MongoDB's native sort-object syntax (`BsonDocument` built directly
      from the `sort` JSON object, preserving key order).
- [x] 2.4 Implement `count: true` handling to return a count instead of streaming documents.
- [x] 2.5 Implement `db` override resolution: per-query `db` key wins, otherwise fall back to the
      connector's `Database` parameter.
- [x] 2.6 Rely on `MongoDB.Bson`'s Extended JSON support (not a hand-rolled parser) for `$oid`/`$date`
      literals within `query`/`aggregate` values.

## 3. GetDataAsync (streaming query execution)

- [x] 3.1 Implement `MongoDbConnector.GetDataAsync` as `async IAsyncEnumerable<ConnectorRow>` per the
      `IConnector`/`ConnectorBase` streaming contract (matching signature incl.
      `[EnumeratorCancellation] CancellationToken`).
- [x] 3.2 Build `ConnectorRow.Columns` once from the first yielded document's shape (top-level field
      names), reused across the stream, consistent with `PostgreSqlConnector`'s pattern.
- [x] 3.3 Map each top-level BSON field to a CLR value in `ConnectorRow.Data`; for subdocument/array
      fields, preserve the nested structure as-is (do not flatten) and set that column's
      `QueryResultColumn.Type` to `QueryResultColumnType.Json`.
- [x] 3.4 Propagate `cancellationToken` through the MongoDB driver's async cursor iteration.

## 4. Schema discovery

- [x] 4.1 Implement `ISchemaDiscovery.GetSchemaAsync()`: list collections in the target database, and
      for each, run `{ $sample: { size: SchemaSampleSize } }` to draw a random document sample.
- [x] 4.2 Build one `DatabaseObject` per collection with `ObjectTypeName = "Collection"`.
- [x] 4.3 For each top-level field observed across a collection's sample, populate a
      `DatabaseObjectColumn`: `TypeName` = consistent BSON type name or `"Mixed"`;
      `Attributes["ObservedTypes"]` when mixed; `Attributes["Presence"]` always (observed fraction,
      e.g. `"42/60"`); nested objects/arrays get `TypeName` = `"Object"`/`"Array"` with no recursive
      field enumeration.

## 5. Connection testing

- [x] 5.1 Implement `IConnectionTest.TestConnectionAsync`: open a connection and run a trivial
      operation (e.g. a server `ping` command), returning `ConnectionTestResult` (`Success`,
      `Message`, `Elapsed`) per the existing `connector-capability-results` contract.

## 6. Client-side Json rendering (Table visualization)

- [x] 6.1 Add a `QueryResultColumnType.Json` branch to `GetTypeForColumnType` in
      `source/TheGrid.Client/Shared/Visualizations/Table.razor.cs`.
- [x] 6.2 Add a `Json`-typed branch to the `<Template>` block in
      `source/TheGrid.Client/Shared/Visualizations/Table.razor`: detect a `JsonElement` with
      `ValueKind` `Object` or `Array`, render a collapsed summary (`{...}`/`[...]`) by default, and
      support click-to-expand into a tree/node view of the value.
- [x] 6.3 Handle a null value for a `Json`-typed column (render empty, no expand affordance).

## 7. Tests

- [x] 7.1 Add `TheGrid.Tests.Connectors` coverage for command-mode selection (find vs. aggregate vs.
      count) using a MongoDB test fixture/container consistent with how other integration-style
      connector tests are set up in this repo.
- [x] 7.2 Add tests for `db` override resolution (per-query override vs. connector default).
- [x] 7.3 Add tests for nested subdocument/array fields mapping to `Json`-typed columns without
      flattening.
- [x] 7.4 Add tests for schema discovery's per-field `TypeName`/`Mixed`/`Presence`/`ObservedTypes`
      inference against a sampled collection with intentionally inconsistent document shapes.
- [x] 7.5 Add a Blazor component test (or manual verification, per existing UI-testing conventions in
      this repo) for `Table`'s collapsed/expand-on-click behavior for `Json`-typed cells.

## 8. Documentation

- [x] 8.1 Update `docs/Creating-Connectors.md` with a MongoDB-connector-derived example alongside the
      existing Postgres one, and note the current staleness in that doc around `IWriteAccessProbe`/
      `ConnectorContext` (pre-existing gap, not introduced by this change) if not already fixed by then.
