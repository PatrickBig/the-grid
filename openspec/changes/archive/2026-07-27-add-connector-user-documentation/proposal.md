## Why

The Grid has two real connectors (`PostgreSqlConnector`, `MongoDbConnector`) and neither has any
user-facing documentation — someone writing a query against either has nothing to read but the C#
source. `docs/Creating-Connectors.md` exists but is explicitly contributor-facing (how to *build* a
connector), not user-facing (how to *query* one that already exists), and CLAUDE.md's newly added
"Documentation" section now requires a `docs/connectors/<ConnectorName>.md` usage doc whenever a
connector is added or its query/schema-discovery behavior changes materially. That rule currently has
no worked example to point to and two connectors already out of compliance with it. This change
establishes the doc convention concretely and queues up writing the two missing docs.

## What Changes

- Establish `docs/connectors/<ConnectorName>.md` as the standard location and shape for connector
  user-facing documentation: what the connector connects to, its connection parameters and their
  meaning, the `Query.Command` contract/query language it accepts, worked examples per execution mode,
  how schema discovery works and how to read its output, and notable capability gaps/caveats. Structure
  each doc as self-contained sections under clear headings so it can plausibly back an in-app help panel
  later — no in-app rendering/API surface is designed or built in this change; plain markdown in the
  repo is the deliverable.
- Queue (as unchecked tasks for a later `/opsx:apply` pass, not written in this change):
  - `docs/connectors/PostgreSQL.md` — plain-SQL `Query.Command` executed via Npgsql, connection
    parameters from `PostgreSqlConnector.cs`'s `[ConnectorParameter]` attributes, `information_schema`-
    based schema discovery, and `IConnectionTest`/`IWriteAccessProbe` support.
  - `docs/connectors/MongoDB.md` — the JSON `Query.Command` contract (`collection`, `query`,
    `aggregate`, `projection`, `sort`, `skip`, `limit`, `count`, `db`, `allowDiskUse`), find-mode vs.
    aggregate-mode selection, the optional `Database` connector parameter with per-query `db` override,
    `$sample`-based schema discovery and how to read `Mixed`/`ObservedTypes`/`Presence`, that nested
    documents/arrays render as collapsed/expandable `Json`-typed cells, and that `IWriteAccessProbe` is
    not implemented for this connector.
- No source code changes — this change (and its follow-up apply pass) is documentation-only.

## Capabilities

### New Capabilities
- `connector-user-documentation`: defines what `docs/connectors/<ConnectorName>.md` must exist and
  accurately cover for each connector shipped in `TheGrid.Connectors`, so the doc-per-connector
  requirement (and its content shape) is itself a checkable requirement rather than untracked prose,
  and future connector changes have a concrete existing requirement to extend.

### Modified Capabilities
(none — this is a new, additive documentation requirement; it does not change any existing spec's
runtime behavior requirements)

## Impact

- **New docs** (queued as tasks, not written here): `docs/connectors/PostgreSQL.md`,
  `docs/connectors/MongoDB.md`.
- **No source code changes.**
- CLAUDE.md's "Documentation" section (already in place) is the enforcement mechanism for keeping
  `docs/connectors/` up to date going forward — this change's spec gives that rule a concrete
  requirement to point at, and does not duplicate or re-litigate it.
