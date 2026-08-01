## Context

The Grid ships two connectors today, `PostgreSqlConnector` and `MongoDbConnector`
(`source/TheGrid.Connectors/*.cs`), and neither has a user-facing usage doc. `docs/Creating-Connectors.md`
covers *building* a connector (base class, attributes, capability interfaces) and is explicitly
contributor-facing; it deliberately does not — and should not — explain what query JSON/SQL a user
writes against an already-built connector, or how to read that connector's schema-discovery output.
That gap is real today: a user configuring a MongoDB connection has no doc describing the `Query.Command`
JSON contract short of reading `MongoDbConnector.cs` or the (internal, rationale-focused)
`openspec/changes/add-mongodb-connector/design.md`.

CLAUDE.md's "Documentation" section (already added, not part of this change's decisions) now requires a
`docs/connectors/<ConnectorName>.md` doc to be created/updated in the same change whenever a connector is
added or its query/schema-discovery behavior changes materially. This change is the first thing to point
at that rule: it establishes the concrete shape/location convention the rule refers to, backs it with a
checkable spec (following the precedent set by `openspec/changes/archive/2026-07-25-rewrite-connector-docs`,
which did the same thing for `docs/Creating-Connectors.md` via the `connector-authoring-guide` capability),
and queues up the two retroactive docs the existing connectors are missing.

This design.md is documentation-structure design, not runtime-behavior design — there is no code impact.

## Goals / Non-Goals

**Goals:**
- Define a standard section shape for `docs/connectors/<ConnectorName>.md` that is audience-correct
  (query author, not connector author), example-driven, and structured so each section could later back
  an in-app help panel without restructuring.
- Make the doc-per-connector requirement a checkable spec requirement (`connector-user-documentation`),
  matching the precedent of `connector-authoring-guide` for the contributor-facing doc.
- Queue `docs/connectors/PostgreSQL.md` and `docs/connectors/MongoDB.md` as concrete, scoped tasks for a
  later `/opsx:apply` pass — content included in this design as the source material that apply pass will
  draw from, but not written into the actual doc files by this change.

**Non-Goals:**
- Building any in-app rendering, API surface, or storage mechanism to actually surface these docs as
  in-app help. Plain markdown in `docs/connectors/` is the complete deliverable of both this change and
  its future apply pass; "written to eventually be surfaced as in-app help" (per CLAUDE.md) only
  constrains *how the markdown is structured* (self-contained sections, clear headings), not what gets
  built now.
- Rewriting or re-scoping `docs/Creating-Connectors.md` — it stays contributor-facing; this change adds a
  sibling, not a replacement.
- Documenting connectors that don't exist yet, or speculatively covering `TestConnector` (an internal
  test-only connector, not part of the connector `[Connector]`-decorated set users configure against real
  data — excluded from doc scope the same way it would be from a connector picker).
- Re-litigating or duplicating the CLAUDE.md Documentation rule itself — it's a settled constraint this
  change satisfies and builds a spec around, not a decision made here.

## Decisions

### 1. Location and naming: `docs/connectors/<ConnectorName>.md`, keyed to the `[Connector]` display name

The filename uses the connector's `[Connector("Name", ...)]` display name (`"PostgreSQL"`, `"MongoDB"`),
not the C# class name (`PostgreSqlConnector`, `MongoDbConnector`) — that display name is what a user
actually sees in the UI when picking a connector, so it's the more discoverable, user-facing identifier
to key the filename on. `docs/connectors/` is a new directory, sibling to the existing `docs/` files
(`Creating-Connectors.md`, `AddingMigrations.md`, `Permissions.md`).

**Alternative considered**: key by class name for a 1:1 mapping with the source file. Rejected — the
audience for this doc never sees `PostgreSqlConnector`, only "PostgreSQL"; optimizing filename
discoverability for the reader (a query author) matters more than mirroring the C# namespace.

### 2. Standard section shape

Every `docs/connectors/<ConnectorName>.md` covers, as self-contained headed sections:

1. **Overview** — what data source/system this connector targets, at a glance.
2. **Connection parameters** — each `[ConnectorParameter]` the connector declares: name, type, whether
   required, and what it means in practice (not just the attribute's own `HelpText`, which is
   necessarily terse).
3. **Query.Command contract** — the query language or JSON shape `GetDataAsync`'s `query` argument
   expects for this connector: plain query-language text for a SQL-shaped connector, or a documented JSON
   schema (keys, types, meaning, which are mutually exclusive/mode-selecting) for a structured-command
   connector like Mongo.
4. **Worked examples** — at least one realistic example per distinct execution mode the connector
   supports (e.g., a single example for Postgres's one mode; separate find-mode and aggregate-mode
   examples for Mongo).
5. **Schema discovery** — whether/how `ISchemaDiscovery.GetSchemaAsync` works for this connector (what
   it queries or samples) and how to interpret its output, including any connector-specific fields (e.g.
   Mongo's `Mixed`/`ObservedTypes`/`Presence`).
6. **Capabilities and caveats** — which optional capability interfaces (`IConnectionTest`,
   `IWriteAccessProbe`) the connector implements, and any notable limitations worth a user knowing before
   they rely on the connector (e.g. a capability the connector deliberately doesn't implement).

This mirrors, at the doc-structure level, the section shape `docs/Creating-Connectors.md` already uses
per capability (parameters, `GetDataAsync`, schema discovery, capability interfaces) — reusing a familiar
shape rather than inventing an unrelated one, while retargeting every section's content at the query
author instead of the connector author.

**Alternative considered**: a single free-form doc per connector with no mandated section list. Rejected
— an unstructured doc is exactly what makes "surface this as in-app help later" hard; mandating headed,
self-contained sections now costs little and keeps that door open without committing to building it.

### 3. Content source of truth per connector

- **PostgreSQL**: `source/TheGrid.Connectors/PostgreSqlConnector.cs` is authoritative for connection
  parameters (the four `[ConnectorParameter]` attributes: Connection String, Database Name, Username,
  Password) and capability interfaces (`ISchemaDiscovery` via `information_schema.columns`/`tables`,
  `IConnectionTest`, `IWriteAccessProbe` via `has_table_privilege`). `Query.Command` is plain SQL text
  executed via Npgsql — no bespoke contract to document beyond "this is SQL, parameterization via
  `queryParameters` is supported."
- **MongoDB**: `source/TheGrid.Connectors/MongoDbConnector.cs` is authoritative for connection parameters
  (Connection String, optional Database, `SchemaSampleSize`) and capability interfaces (`ISchemaDiscovery`
  via `$sample`, `IConnectionTest`; `IWriteAccessProbe` is **not** implemented — a deliberate,
  already-documented trade-off, not an oversight). `openspec/changes/add-mongodb-connector/design.md` is
  authoritative for the `Query.Command` JSON contract's *shape and rationale* (find vs. aggregate mode
  selection, `db` override resolution, native sort-object syntax, `$sample`-based schema inference and its
  `Mixed`/`ObservedTypes`/`Presence` fields) — but the user-facing doc restates that contract as a query
  reference (keys, types, worked examples), not the design rationale itself; *why* `sort` uses native
  syntax instead of Redash's array dialect stays in `design.md`, out of the user-facing doc. Both the
  connector source and `design.md` already use the current key name `projection` (renamed from an earlier
  `fields`); the future apply pass must use `projection` throughout, matching current code — there is no
  remaining `fields` reference to reconcile.

### 4. Enforcement going forward: the existing CLAUDE.md rule, backed by this change's spec

This change does not invent a new enforcement mechanism. CLAUDE.md's Documentation section already
requires a `docs/connectors/<ConnectorName>.md` create/update in the same change as any connector
addition or material query/schema-discovery behavior change. What this change adds is the concrete
target that rule was written to point at (the convention in Decision 2) plus a checkable spec
(`connector-user-documentation`) — following the same pattern `connector-authoring-guide` set for
`docs/Creating-Connectors.md` — so a future connector-changing proposal has an existing requirement to
write a delta against, rather than the CLAUDE.md rule being the only trace of the obligation.

## Risks / Trade-offs

- **[Risk] The two retroactive docs (PostgreSQL, MongoDB) are only queued as tasks here, not written —
  this change alone does not close the compliance gap it describes.** → Mitigation: intentional split per
  the task instructions (propose-only now, content in a dedicated `/opsx:apply` pass); the tasks are
  scoped precisely enough (content source of truth identified per connector in Decision 3) that the apply
  pass is close to mechanical, not open-ended research.
- **[Risk] Docs drift from connector behavior over time, same risk `connector-authoring-guide` already
  named for the contributor-facing doc.** → Mitigation: the CLAUDE.md rule plus this change's spec give
  every future connector-behavior change an explicit, checkable doc obligation; no new mitigation invented
  here beyond making that obligation concrete.
- **[Trade-off] Doc structure is designed with an eye toward eventual in-app help rendering, but no
  rendering is built, so that goal is unverified until something actually consumes these docs.** →
  Accepted: over-structuring markdown slightly now is cheap; building unused rendering infrastructure
  speculatively would not be.

## Migration Plan

None — additive documentation convention plus two queued docs; no code, schema, or data affected.

## Open Questions

- Should `docs/connectors/` eventually include a doc for `TestConnector` (used in integration tests) for
  contributor convenience, even though it's excluded from user-facing scope here? Left open; not needed
  for this change since `TestConnector` isn't a real user-facing data source.
