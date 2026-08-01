## Context

The connector SDK (`TheGrid.Connectors.Abstractions` + `TheGrid.Connectors`) has one real
implementation today: `PostgreSqlConnector`. Every abstraction in the SDK — `IConnector.GetDataAsync`,
`ISchemaDiscovery.GetSchemaAsync`, `ConnectorRow`, `DatabaseSchema`/`DatabaseObject`/
`DatabaseObjectColumn` — was designed against a single relational, strongly-schematized data source. A
second RDBMS (e.g. SQL Server) would validate almost none of these abstractions further, since it looks
structurally identical to Postgres from the SDK's point of view. MongoDB is chosen specifically because
it's a document store with no fixed schema, which forces real decisions in three places: what
`Query.Command` even means without a query language string, how "schema discovery" works without a
catalog to query, and whether one document safely maps to one flat `ConnectorRow`.

`ConnectorBase` (primary-constructor form, taking a `ConnectorContext`), `IConnectorFactory`,
`ISchemaDiscovery`, `IConnectionTest`, and `IWriteAccessProbe` are all current (post-P1-3/P1-5)
contracts — this design targets those, not the older reflection-based/`IPermissionTest` shapes still
shown in `docs/Creating-Connectors.md` (that doc predates the P1 SDK changes and is stale in places;
updating it is a candidate follow-up task, not blocking this change).

## Goals / Non-Goals

**Goals:**
- Define a `Query.Command` JSON contract for MongoDB that supports both `.find()` and `.aggregate()`
  execution, modeled on Redash's prior art but improved where the improvement is low-risk and
  well-justified (native sort syntax).
- Implement schema discovery for a schemaless store via a simple, tunable sampling strategy.
- Correctly represent MongoDB's document/nested-value shape within the existing flat `ConnectorRow`
  contract, extending column typing (not row shape) to carry the nested-ness signal.
- Make `Json`-typed result cells actually usable in the UI (today they're inert).

**Non-Goals:**
- Schema caching, scheduled refresh, an explorer UI, or refresh-race handling — cross-cutting concern
  affecting every connector, not specific to Mongo; deferred to a separate change.
- A general cross-connector query-parameter templating system, or a general "dynamic value function"
  abstraction (`$humanTime`-equivalent) — both deferred pending a second connector that would actually
  need them, to avoid designing an abstraction against a single data point.
- Write-access probing (`IWriteAccessProbe`) for Mongo — no cheap, generally-applicable analog to
  Postgres's `has_table_privilege` exists across Mongo's role-based permission model; left for later.
- Flattening nested documents into dotted-path pseudo-columns. A user who wants that writes a
  `$project` stage themselves; the connector doesn't try to infer a flattening.

## Decisions

### 1. `Query.Command` is a JSON object, mode inferred from which keys are present

```json
// find() mode
{
  "collection": "orders",
  "query": { "status": "shipped" },
  "projection": { "_id": 1, "status": 1, "total": 1 },
  "sort": { "orderedAt": -1 },
  "skip": 0,
  "limit": 100
}

// aggregate() mode
{
  "collection": "orders",
  "aggregate": [
    { "$match": { "status": "shipped" } },
    { "$group": { "_id": "$customerId", "total": { "$sum": "$total" } } },
    { "$sort": { "total": -1 } }
  ],
  "allowDiskUse": true
}
```

Presence of `aggregate` selects `.aggregate()`; otherwise `.find()` is used (`query`/`projection`/`sort`
apply). `count: true` runs a count instead of returning documents. `db` optionally overrides the
connection's default database for that one query (see Decision 3). MongoDB Extended JSON (`$oid`,
`$date`, etc.) is supported within `query`/`aggregate` values for typed literals, since that's standard
MongoDB JSON tooling behavior, not a Grid invention.

**Alternative considered**: a raw shell-syntax string (`db.orders.find({...})`), closer to how users
write ad-hoc Mongo queries interactively. Rejected — parsing arbitrary MongoDB shell syntax (which is
JavaScript, not JSON) is a much larger undertaking than accepting structured JSON, and Redash's own
prior art (JSON object) proves the JSON-object approach is sufficient for a query-tool UI where users
are filling in a structured editor, not a REPL.

### 2. `sort` uses native MongoDB sort-object syntax, not Redash's ordered array

Redash represents `sort` as `[{"name": "date", "direction": -1}]` — an explicit ordered array — rather
than MongoDB's native `{"date": -1}` object form. That choice is best explained as a defensive move
against JSON parsers that don't guarantee object key order for multi-field sorts.

`System.Text.Json`, which this project uses throughout, *does* preserve object property order on both
parse and serialize. There's no correctness reason to avoid the native form here, and using it means
one less bespoke dialect for anyone who already knows MongoDB to learn. This is a deliberate,
narrow improvement over Redash's design — not a wholesale re-design of the contract.

### 3. Database scoping: optional connector-level default + per-query override

Postgres's `DatabaseName` connector parameter is effectively required — a Postgres connection targets
exactly one database. MongoDB connections are more naturally cluster/deployment-scoped; picking a
database per query is idiomatic Mongo usage. This connector adds `Database` as an **optional**
`[ConnectorParameter]` (unlike Postgres's required one) used as the default when a query's `db` key is
absent, so single-database use (the common case, per prior discussion) doesn't require repeating `db`
in every query, while multi-database use is still fully supported.

### 4. Schema discovery: `$sample`-only MVP, no oldest/newest/middle sampling

Populating `DatabaseSchema` for a Mongo connection samples each collection via
`{ $sample: { size: N } }`, where `N` is a tunable `SchemaSampleSize` connector parameter (numeric,
sensible default e.g. 100) — same attribute pattern as `TestConnector`'s `numberOfRows`.

An earlier design pass considered adding oldest-N/newest-N-by-`_id` sampling (cheap index scans) to
also catch schema drift over a collection's lifetime, dropping only a "middle" slice (which requires an
expensive `skip()` — O(n) with no cheap random-offset access in MongoDB). That was deliberately cut
from this change's scope: MongoDB schema inference is a genuinely deep problem (entire products exist
just to do it well), and a single random sample is a reasonable, honest MVP. Temporal-drift-aware
sampling can be added later if `$sample` alone proves insufficient in practice — there's no evidence
yet that it will.

For each top-level field observed across a collection's sample, the resulting `DatabaseObjectColumn`
gets:
- `TypeName`: the BSON type name if consistent across all samples where the field is present, else
  `"Mixed"`.
- `Attributes["ObservedTypes"]`: set only when `TypeName == "Mixed"`, e.g. `"Int32, String"`.
- `Attributes["Presence"]`: always set, e.g. `"42/60"` — the fraction of sampled documents containing
  the field at all, since MongoDB fields are inherently optional. This is likely the most actionable
  signal for a user deciding whether a field is core or an edge case.
- Nested objects/arrays: `TypeName` = `"Object"` / `"Array"` (no recursive field enumeration — the
  contents stay opaque at the schema level too, consistent with Decision 5's row-mapping choice).

Each MongoDB collection becomes one `DatabaseObject` with `ObjectTypeName = "Collection"` (already a
suggested value in that model's doc comments).

### 5. One document = one row; nested values stay as opaque `Json`-typed cells

Each document a query returns maps to one `ConnectorRow`. A nested subdocument or array value is kept
as-is in `ConnectorRow.Data` (not flattened into `field.subfield`-style pseudo-columns), and the
corresponding `QueryResultColumn.Type` is set to `QueryResultColumnType.Json` — an enum value that
already exists (added under `query-result-type-mapping` for CLR `JsonElement`/`JsonDocument` mapping)
but has had no real producer until now.

This is a direct consequence of Decision 4 (no recursive schema flattening) applied at the row level
too, and keeps `ConnectorRow`'s flat `IReadOnlyDictionary<string, object?>` shape completely unchanged
— no SDK model changes needed, just a connector correctly using an existing column type.

**Consequence requiring a client-side fix**: `Table.razor`/`Table.razor.cs` currently has no handling
for `QueryResultColumnType.Json` at all — `GetTypeForColumnType` falls through to `typeof(string)`, and
the `<Template>` block's only branches are `Text`/no-`DisplayFormat` (raw render) and a few
numeric/date `DisplayFormat` cases, defaulting to `@value` otherwise. Tracing how a `Json`-typed cell
actually arrives client-side: `QueryDataConverter.Read` (`TheGrid.Shared/Utilities/QueryDataConverter.cs`)
clones the token into a `System.Text.Json.JsonElement` for any object/array-shaped JSON value — so
`context[kv.Key]` for a Mongo nested-document column is a `JsonElement` with `ValueKind` `Object` or
`Array` by the time it reaches the grid. Today that falls to the generic `@value` branch, which prints
`JsonElement`'s `ToString()` — which for object/array kinds happens to return the raw JSON text, so
results are technically visible today, just as an unstyled unbroken JSON blob inline in the cell. This
change adds a real `Json` branch: collapsed by default (e.g. `{...}` / `[...]`), expandable on click
into a tree/node view of the `JsonElement`'s contents. This is scoped as part of this change (not
deferred) because without it, Mongo query results are effectively unreadable for any query touching
nested data — which, for a document store, is most of them.

## Risks / Trade-offs

- **[Risk] `$sample`-only schema discovery may miss rare or legacy field shapes in collections with
  significant schema drift** → Mitigation: `Presence` and `ObservedTypes` attributes make the
  sample's limitations visible to the user rather than hiding them; `SchemaSampleSize` is tunable per
  connection; documented as a known MVP limitation, not silently over-promised.
- **[Risk] Command JSON round-trips through Extended JSON for typed literals (`$oid`, `$date`), which
  is an easy thing to get subtly wrong (e.g. conflating a literal `$oid` key in user data with the
  Extended JSON operator)** → Mitigation: rely on the official MongoDB .NET driver's own Extended JSON
  parsing (`MongoDB.Bson`/`MongoDB.Driver`) rather than hand-rolling a parser, so this class of bug is
  the driver's problem, not new code in this connector.
- **[Trade-off] No `IWriteAccessProbe` implementation means Mongo connections can't be flagged
  read-only/write-capable the way Postgres connections can** → Accepted for this change; revisit once
  a reasonable cross-connection-type approach (or a Mongo-specific one) is worth the design time.
- **[Trade-off] Optional `Database` connector parameter means a connection can be configured with
  neither a default database nor a per-query `db`, producing a MongoDB driver-level error only at query
  execution time** → Accepted: this mirrors how other required-at-runtime-but-not-required-at-declare-time
  gaps already surface (e.g. a malformed connection string), and keeping `Database` optional is
  necessary to support the common multi-database-per-connection case from Decision 3.

## Open Questions

- Should `SchemaSampleSize` apply uniformly across all collections in a database, or should it be
  possible to tune per-collection? This change assumes uniform (simplest), revisit if real usage shows
  some collections need a much larger/smaller sample than others.
