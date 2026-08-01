# MongoDB connector

## Overview

The MongoDB connector runs queries against a MongoDB database — a document store, not a relational
database. Instead of SQL, `Query.Command` is a JSON object describing either a `find()`-style filter or
an `aggregate()` pipeline to run against a single collection. Use it for connections that point at a
MongoDB deployment (self-hosted or Atlas).

## Connection parameters

| Parameter           | Type              | Required | Meaning |
|----------------------|-------------------|----------|---------|
| Connection String     | Single-line text  | Yes | A standard MongoDB connection string, e.g. `mongodb://user:password@host:27017`. |
| Database               | Single-line text  | No  | The default database used when a query's command JSON doesn't include its own `db` key. Because a MongoDB connection is naturally cluster/deployment-scoped (unlike Postgres, where a connection targets exactly one database), this is optional — leave it unset if every query will specify its own `db`. |
| Schema Sample Size    | Numeric           | No  | Number of documents sampled per collection (via `$sample`) when schema discovery runs. Defaults to 100 if unset. |

## Query.Command contract

`Query.Command` is a JSON object, not a query-language string. It always identifies a target
`collection`, plus a set of keys that select and configure one of two execution modes.

| Key            | Mode           | Meaning |
|-----------------|----------------|---------|
| `collection`     | both           | **Required.** The name of the collection to query. |
| `query`           | find           | Filter document, in native MongoDB query syntax (e.g. `{ "status": "shipped" }`). Omit for no filter. |
| `projection`      | find           | Projection document controlling which fields are returned (e.g. `{ "_id": 1, "status": 1 }`). |
| `sort`             | find           | Sort document, in **native MongoDB sort-object syntax** — `{ "field": 1, "field2": -1 }` (`1` ascending, `-1` descending) — not an ordered array of `{name, direction}` objects. |
| `skip`             | find           | Number of matching documents to skip before returning results. |
| `limit`            | find           | Maximum number of documents to return. |
| `aggregate`        | aggregate      | Array of aggregation pipeline stage objects (e.g. `{ "$match": ... }`, `{ "$group": ... }`). **Presence of this key selects aggregate mode**, regardless of whether `query` is also present in the command — `query`/`projection`/`sort`/`skip`/`limit` are ignored in that case. |
| `allowDiskUse`     | aggregate      | Boolean. Passed through to the aggregation as `AllowDiskUse`, for pipelines that need to spill large intermediate stages to disk. |
| `count`            | both           | Boolean. When `true`, the query returns a single row with one `count` column (the match/pipeline count) instead of streaming documents. In aggregate mode this appends a `$count` stage to your pipeline; in find mode it counts documents matching `query`. |
| `db`               | both           | Per-query database override. See "Database resolution" below. |

Extended JSON literals (`$oid`, `$date`, etc.) are supported anywhere inside `query`/`aggregate`, since
the command JSON is parsed with the MongoDB driver's own BSON/Extended JSON parser — the same syntax
you'd use in `mongosh` or Compass works here.

### Mode selection

- **Aggregate mode** runs whenever the command JSON includes an `aggregate` key (an array of pipeline
  stages), regardless of whether a `query` key is also present.
- **Find mode** runs otherwise, using `query` (or an empty filter, matching every document, if `query`
  is omitted), plus optional `projection`/`sort`/`skip`/`limit`.

### Database resolution

Each query resolves which database to run against as follows, in order:

1. The command JSON's own `db` key, if present, wins.
2. Otherwise, the connection's **Database** parameter is used, if it's set.
3. If neither is set, the query fails at execution time with an error asking you to set one or the
   other — there's no further fallback.

## Worked examples

### Find mode

```json
{
  "collection": "orders",
  "query": { "status": "shipped" },
  "projection": { "_id": 1, "status": 1, "total": 1 },
  "sort": { "orderedAt": -1 },
  "limit": 100
}
```

Returns up to 100 shipped orders, newest first, with only `_id`/`status`/`total` returned per document.

### Aggregate mode

```json
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

Groups shipped orders by customer, summing `total` per customer, sorted highest-spend first.

### Count

```json
{
  "collection": "orders",
  "query": { "status": "shipped" },
  "count": true
}
```

Returns a single row with one `count` column, instead of the matching documents themselves.

## Schema discovery

The MongoDB connector implements `ISchemaDiscovery` via random sampling — there's no schema catalog to
query the way `information_schema` exists for Postgres, since MongoDB documents in the same collection
can each have a different shape.

Schema discovery always runs against the connection's default database — the **Database** connector
parameter — and, unlike a query, has no per-call `db` override. If **Database** isn't set on the
connection, schema discovery fails with an error asking you to set it, even if every query you run
against this connection supplies its own `db` key.

For each collection in that database, `GetSchemaAsync` draws a random sample of documents via
`{ $sample: { size: N } }`, where `N` is the **Schema Sample Size** connector parameter (default 100).
Each collection becomes one `DatabaseObject` with object type `Collection`. Every top-level field
observed anywhere in the sample becomes one column, with:

- **`TypeName`**: the BSON type name (e.g. `String`, `Int32`, `Boolean`, `ObjectId`), if every sampled
  document that has this field agrees on its type. If the field's type varies across the sample,
  `TypeName` is `Mixed` instead.
- **`Attributes["ObservedTypes"]`**: only set when `TypeName` is `Mixed` — a comma-separated list of the
  types actually observed, e.g. `"Int32, String"`.
- **`Attributes["Presence"]`**: always set, e.g. `"42/60"` — the number of sampled documents that had
  this field at all, out of the sample size actually drawn. Since MongoDB fields are inherently
  optional, this is generally the most useful signal for judging whether a field is core to the
  collection or an edge case.
- **Nested objects/arrays**: typed `Object` or `Array` respectively, with no recursion into their
  contents — a nested subdocument's own fields are not separately enumerated in the schema output.

Because this is a sample, not an exhaustive scan, a field that's rare or was only present in old/legacy
documents may not show up at all (or may show a `Presence` that understates its real frequency) if it
didn't happen to land in the sampled documents. Increase **Schema Sample Size** if you need more
confidence for a collection with a lot of shape variation, keeping in mind a larger sample costs more to
compute.

## Results rendering

Query results map one MongoDB document to one row. Scalar fields (strings, numbers, booleans, dates,
ObjectIds, binary data) map to their natural column type. Nested subdocuments and arrays are **not**
flattened into separate columns — they stay as a single `Json`-typed column, rendered in the results
table as a collapsed `{...}` or `[...]` cell that expands into a tree view when clicked. If you want a
nested value broken out into its own column instead, write a `$project` stage (or a `projection`
document) that pulls the specific value you want up to the top level.

## Capabilities and caveats

- **`IConnectionTest`**: implemented. Testing a connection runs a server `ping` command against the
  connection's default database (or `admin` if no default database is configured).
- **`IWriteAccessProbe`**: **not implemented** for this connector. This is a deliberate choice, not a
  gap that's just been missed — MongoDB's role-based permission model has no single cheap query
  equivalent to Postgres's `has_table_privilege` that can answer "can this connection write here"
  across arbitrary collections. As a result, Mongo connections cannot currently be automatically
  flagged as read-only vs. write-capable the way Postgres connections can; if that distinction matters
  for a given connection, verify the configured user's roles directly in MongoDB.
