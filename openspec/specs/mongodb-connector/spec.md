# mongodb-connector Specification

## Purpose

TBD - created by syncing change add-mongodb-connector. Update Purpose after archive.

## Requirements

### Requirement: Command JSON selects find or aggregate execution mode
`Query.Command` for a MongoDB connection SHALL be a JSON object identifying a target `collection` plus
either a `query` (optionally with `projection`, `sort`, `skip`, `limit`, `count`) for `.find()`-mode
execution, or an `aggregate` array of pipeline stages for `.aggregate()`-mode execution. The presence of
an `aggregate` key SHALL select aggregate mode; its absence SHALL select find mode.

#### Scenario: A command with a query key runs as find
- **WHEN** a query's `Command` JSON contains a `query` key and no `aggregate` key
- **THEN** the connector executes the command against the named `collection` using `.find()`

#### Scenario: A command with an aggregate key runs as aggregate
- **WHEN** a query's `Command` JSON contains an `aggregate` key
- **THEN** the connector executes the command against the named `collection` using
  `.aggregate()` with the given pipeline stages, regardless of whether a `query` key is also present

#### Scenario: A count-only command returns a count instead of documents
- **WHEN** a query's `Command` JSON sets `"count": true` in find mode
- **THEN** the connector returns a single count value rather than streaming matched documents

### Requirement: Sort uses native MongoDB sort-object syntax
The `sort` key in find-mode command JSON SHALL use MongoDB's native sort-object syntax
(`{"field": 1, "field2": -1}`) rather than an ordered array of separate name/direction objects.

#### Scenario: A multi-field sort preserves field order
- **WHEN** a command's `sort` value is `{"status": 1, "orderedAt": -1}`
- **THEN** the connector applies the sort with `status` as the primary key ascending and `orderedAt` as
  the secondary key descending, matching the order the keys appear in the JSON object

### Requirement: Query database resolves from a per-query override or a connector default
A MongoDB connector SHALL expose an optional `Database` connector parameter used as the default target
database for queries. A query's command JSON MAY specify a `db` key to target a different database for
that query only, overriding the connector's default.

#### Scenario: A query without a db key uses the connector's default database
- **WHEN** a connection has `Database` set to `"analytics"` and a query's command JSON has no `db` key
- **THEN** the query executes against the `analytics` database

#### Scenario: A query's db key overrides the connector default
- **WHEN** a connection has `Database` set to `"analytics"` and a query's command JSON sets
  `"db": "archive"`
- **THEN** the query executes against the `archive` database, not `analytics`

### Requirement: Schema discovery samples each collection via $sample
`GetSchemaAsync` on a MongoDB connector SHALL populate one `DatabaseObject` (with
`ObjectTypeName` set to `"Collection"`) per collection in the target database, using MongoDB's
`{ $sample: { size: N } }` aggregation stage to draw a random sample of documents from each collection,
where `N` is a tunable `SchemaSampleSize` connector parameter.

#### Scenario: Each collection becomes one DatabaseObject
- **WHEN** `GetSchemaAsync` runs against a database containing collections `orders` and `customers`
- **THEN** the returned `DatabaseSchema.DatabaseObjects` contains one `DatabaseObject` per collection,
  each with `ObjectTypeName == "Collection"`

#### Scenario: Sample size is controlled by a connector parameter
- **WHEN** a connection's `SchemaSampleSize` parameter is set to `50`
- **THEN** schema discovery for each collection samples 50 documents via `$sample`, not a fixed or
  hardcoded count

### Requirement: Sampled fields record inferred type and presence
For each top-level field observed across a collection's sampled documents, the corresponding
`DatabaseObjectColumn` SHALL record a `TypeName` (the BSON type name if consistent across all samples
containing the field, or `"Mixed"` if it varies) and an `Attributes["Presence"]` value expressing the
fraction of sampled documents that contained the field. When `TypeName` is `"Mixed"`,
`Attributes["ObservedTypes"]` SHALL list the distinct types observed.

#### Scenario: A consistently-typed field gets its BSON type name
- **WHEN** a sampled field is present as a string value in every sampled document that has it
- **THEN** its `DatabaseObjectColumn.TypeName` is the string BSON type name, with no `ObservedTypes`
  attribute set

#### Scenario: A field with inconsistent types across samples is marked Mixed
- **WHEN** a sampled field is an integer in some sampled documents and a string in others
- **THEN** its `DatabaseObjectColumn.TypeName` is `"Mixed"` and `Attributes["ObservedTypes"]` lists both
  observed types

#### Scenario: A field present in only some sampled documents records its presence fraction
- **WHEN** a sampled field appears in 42 of 60 sampled documents
- **THEN** its `DatabaseObjectColumn.Attributes["Presence"]` reflects that fraction (e.g. `"42/60"`)

#### Scenario: Nested objects and arrays are typed without recursing into their contents
- **WHEN** a sampled field's value is a subdocument or an array in the sampled documents
- **THEN** its `DatabaseObjectColumn.TypeName` is `"Object"` or `"Array"` respectively, and no nested
  fields within it are separately enumerated as their own `DatabaseObjectColumn` entries

### Requirement: Each document maps to one row with nested values preserved as Json-typed cells
`GetDataAsync` on a MongoDB connector SHALL yield exactly one `ConnectorRow` per matched document.
A field whose value is a subdocument or array SHALL be preserved as-is in `ConnectorRow.Data` (not
flattened into separate dotted-path columns), and its corresponding `QueryResultColumn.Type` SHALL be
`QueryResultColumnType.Json`.

#### Scenario: A document with a nested subdocument yields one row with one Json-typed cell
- **WHEN** a matched document has a top-level `address` field whose value is a subdocument
- **THEN** the yielded `ConnectorRow` has a single `address` entry in `Data` holding the subdocument's
  raw structure, and `Columns["address"].Type` is `QueryResultColumnType.Json`

#### Scenario: A document with an array field yields one row with one Json-typed cell
- **WHEN** a matched document has a top-level `tags` field whose value is an array
- **THEN** the yielded `ConnectorRow` has a single `tags` entry in `Data` holding the array's raw
  structure, and `Columns["tags"].Type` is `QueryResultColumnType.Json`
