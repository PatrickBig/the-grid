## 1. Setup

- [x] 1.1 Create the `docs/connectors/` directory.

## 2. Write docs/connectors/PostgreSQL.md

- [x] 2.1 Overview section: what PostgreSQL/what class of data source this connector targets.
- [x] 2.2 Connection parameters section, sourced from `PostgreSqlConnector.cs`'s `[ConnectorParameter]`
      attributes: Connection String (required, with a note that it's a standard PostgreSQL connection
      string), Database Name (required), Username (required), Password (required, protected/masked
      input). Note that Username/Password/Database Name values override the parsed Connection String's
      corresponding fields (see `PostgreSqlConnector.GetConnection`).
- [x] 2.3 `Query.Command` contract section: state plainly that `Query.Command` is plain SQL text executed
      via Npgsql (no bespoke JSON contract), and that `queryParameters` (when supplied by the caller)
      are bound as parameterized query values.
- [x] 2.4 Worked example: one realistic `SELECT` query example (the connector's only execution mode).
- [x] 2.5 Schema discovery section: explain that `GetSchemaAsync` queries `information_schema.columns`/
      `information_schema.tables` (excluding `pg_catalog`/`information_schema` schemas themselves),
      producing one `DatabaseObject` per table/view with columns carrying `TypeName` (from
      `udt_name`, with `[]` appended for array types) and attributes (`Nullable`, `Max Length`,
      `Identity`) where applicable.
- [x] 2.6 Capabilities and caveats section: `IConnectionTest` (a trivial `select 1`), `IWriteAccessProbe`
      (checks `has_table_privilege` for INSERT/UPDATE/DELETE across the current schema's tables to flag
      write-capable connections).
- [x] 2.7 Cross-check every parameter, behavior, and capability claim in the finished doc directly
      against `source/TheGrid.Connectors/PostgreSqlConnector.cs` (not from memory/paraphrase) before
      considering this doc done.

## 3. Write docs/connectors/MongoDB.md

- [x] 3.1 Overview section: what MongoDB/document-store class of data source this connector targets.
- [x] 3.2 Connection parameters section, sourced from `MongoDbConnector.cs`'s `[ConnectorParameter]`
      attributes: Connection String (required, standard `mongodb://` URI), Database (optional — default
      database used when a query's command JSON has no `db` key), Schema Sample Size (optional, numeric,
      default 100 — number of documents sampled per collection via `$sample` during schema discovery).
- [x] 3.3 `Query.Command` contract section, sourced primarily from
      `openspec/changes/add-mongodb-connector/design.md` restated as a query reference (not its design
      rationale): document each key — `collection` (required), `query` (find-mode filter), `aggregate`
      (array of pipeline stages; presence selects aggregate mode over find mode), `projection` (find-mode
      projection document — use this exact key name, not `fields`), `sort` (find-mode sort, native
      MongoDB sort-object syntax, e.g. `{"field": 1, "field2": -1}`), `skip`/`limit` (find-mode paging),
      `count` (boolean; when true, returns a single count row instead of documents), `db` (per-query
      database override), `allowDiskUse` (aggregate-mode option). State the find-vs-aggregate mode
      selection rule explicitly: presence of `aggregate` selects aggregate mode regardless of whether
      `query` is also present.
- [x] 3.4 Worked example: a find-mode example (e.g. filter + projection + sort + limit) and a separate
      aggregate-mode example (e.g. a `$match`/`$group`/`$sort` pipeline), matching the two distinct
      execution modes.
- [x] 3.5 Document the `db` override precedence: a query's own `db` key wins; otherwise the connection's
      `Database` parameter is used; if neither is set, execution fails at query time.
- [x] 3.6 Schema discovery section: explain `$sample`-based discovery (one `DatabaseObject` per
      collection, `ObjectTypeName = "Collection"`, sample size from the Schema Sample Size parameter),
      and how to read a discovered column's fields — `TypeName` (consistent BSON type name, or `"Mixed"`
      when it varies across the sample), `Attributes["ObservedTypes"]` (set only when `Mixed`, e.g.
      `"Int32, String"`), `Attributes["Presence"]` (always set, e.g. `"42/60"` — fraction of sampled
      documents containing the field), and that nested objects/arrays are typed `"Object"`/`"Array"`
      without recursing into their contents.
- [x] 3.7 Results rendering note: nested documents/arrays in query results are `Json`-typed columns that
      render as collapsed (`{...}`/`[...]`) cells in the results table, expandable into a tree view on
      click — not flattened into separate columns.
- [x] 3.8 Capabilities and caveats section: `IConnectionTest` (a server `ping`); explicitly state
      `IWriteAccessProbe` is **not** implemented for this connector (a deliberate trade-off — Mongo's
      role-based permission model has no single cheap "can I write here" query the way Postgres's
      `has_table_privilege` does — see `openspec/changes/add-mongodb-connector/design.md`), so Mongo
      connections cannot be flagged read-only/write-capable the way Postgres connections can.
- [x] 3.9 Cross-check every parameter, JSON key, and capability claim in the finished doc directly against
      `source/TheGrid.Connectors/MongoDbConnector.cs` and `openspec/changes/add-mongodb-connector/design.md`
      (not from memory/paraphrase) before considering this doc done, and confirm `projection` (not
      `fields`) is used consistently throughout.

## 4. Verify

- [x] 4.1 Confirm both docs satisfy every requirement/scenario in
      `openspec/changes/add-connector-user-documentation/specs/connector-user-documentation/spec.md`
      (doc exists at the expected path, standard sections present, query-author-only content, at least
      one worked example per execution mode, schema discovery section present and accurate).
- [x] 4.2 Grep both new docs for stray references to design-rationale language ("we chose", "rejected",
      "alternative considered") that belongs in `design.md`, not a user-facing doc — remove any found.
