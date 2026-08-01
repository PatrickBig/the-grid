# PostgreSQL connector

## Overview

The PostgreSQL connector runs queries against a PostgreSQL database (or any Postgres-wire-compatible
server) using [Npgsql](https://www.npgsql.org/). Use it for connections that point at a relational
Postgres database — `Query.Command` is plain SQL, executed as-is against the configured database.

## Connection parameters

| Parameter         | Type            | Required | Meaning |
|--------------------|-----------------|----------|---------|
| Connection String  | Single-line text | Yes | A standard [PostgreSQL connection string](https://www.connectionstrings.com/postgresql/) (e.g. `Host=myserver;Port=5432;Database=mydb;Username=user;Password=pass`). This is parsed as the base connection, and the fields below can override individual parts of it. |
| Database Name      | Single-line text | Yes | The database to connect to. Overrides the `Database` portion of the parsed Connection String. |
| Username            | Single-line text | Yes | The Postgres role to authenticate as. Overrides the `Username` portion of the parsed Connection String. |
| Password            | Protected text (masked) | Yes | The password for the given Username. Overrides the `Password` portion of the parsed Connection String. |

All four parameters are required to save a connection. In practice you can put a complete connection
string in **Connection String** and then use **Database Name**/**Username**/**Password** to
override specific pieces of it (for example, keeping a shared connection string template but
substituting per-environment credentials) — whatever you set in those three fields wins over what's
parsed out of the connection string.

## Query.Command contract

For this connector, `Query.Command` is plain SQL text — there is no bespoke JSON contract to learn.
Whatever you write is sent to the server via Npgsql as-is (e.g. `SELECT`, and any other statement your
connection's role is permitted to run).

The connector itself supports binding named parameters (`@paramName`-style placeholders) safely instead
of string-concatenating values into the SQL text, if a caller supplies a name/value map alongside the
query. **The Grid does not yet expose a way to supply those values through the query editor or API** —
there's no UI/API path today for setting `@paramName` values per execution, so treat this as a capability
the connector is ready for, not something you can use from a query yet.

## Worked example

```sql
SELECT
    o.id,
    o.status,
    o.total,
    o.ordered_at
FROM orders AS o
WHERE o.status = 'shipped'
ORDER BY o.ordered_at DESC
LIMIT 100;
```

This is the connector's only execution mode — there's no separate "find" vs. "aggregate" distinction
like a document-store connector might have; every query is just SQL.

## Schema discovery

The PostgreSQL connector implements `ISchemaDiscovery`. `GetSchemaAsync` queries
`information_schema.columns` joined to `information_schema.tables`, excluding the `pg_catalog` and
`information_schema` schemas themselves (so only user-created schemas/tables/views are returned).

The result is one `DatabaseObject` per table or view, in `schema.table` order, each carrying its
columns:

- **Object type**: `TABLE` for base tables (the underlying `information_schema` value `BASE TABLE` is
  normalized to `TABLE`), or the raw `table_type` value otherwise (e.g. `VIEW`).
- **Column `TypeName`**: derived from `udt_name` (Postgres's underlying/user-defined type name). For
  array columns (`data_type = 'ARRAY'`), leading/trailing underscores are trimmed from the element type
  name and `[]` is appended — e.g. an `integer[]` column shows as `int4[]`.
- **Column attributes** (present only when applicable):
  - `Nullable` — present (with no value) when the column allows `NULL`.
  - `Max Length` — the column's `character_maximum_length`, when the type has one (e.g. `varchar(255)`
    shows `Max Length = 255`).
  - `Identity` — present (with no value) when the column is an identity column.

## Capabilities and caveats

- **`IConnectionTest`**: implemented. Testing a connection opens it and runs a trivial `select 1` to
  confirm the configured parameters actually connect.
- **`IWriteAccessProbe`**: implemented. This checks, for every table in the connection's current
  schema, whether `has_table_privilege` reports `INSERT`, `UPDATE`, or `DELETE` access. If any table
  grants any of those privileges to the configured role, the connection is flagged as write-capable —
  useful for confirming a connection meant to be read-only actually is.
- No bespoke query language or execution modes to be aware of: `Query.Command` is exactly the SQL text
  you'd run in any other Postgres client, so anything valid there (subject to the configured role's
  permissions) is valid here.
