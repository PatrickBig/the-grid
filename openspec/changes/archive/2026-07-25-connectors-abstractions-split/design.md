## Context

`TheGrid.Connectors.csproj` today references `Npgsql`, `Dapper` (unused — grepped, zero references
anywhere in the project), and `Mapster` (used only by `Extensions/ConnectorExtensions.cs`). Everything —
interfaces, attributes, models, the concrete `PostgreSqlConnector`/`TestConnector`, and the extension
helpers — lives in one assembly. Two projects reference it directly: `TheGrid.Services.csproj` and
`tests/TheGrid.Tests.Connectors.csproj`. `TheGrid.Server` gets it transitively through its reference to
`TheGrid.Services`.

**The critical discovery while grounding this change:** three separate call sites resolve "the assembly
containing connector implementations" via `System.Reflection.Assembly.GetAssembly(typeof(IConnector))`:
`ConnectorDiscoveryService.GetConnectorTypes()`, `QueryExecutor.GetConnector()`, and
`ConnectionsController.ResolveConnectorType()` (this third one wasn't mentioned in
`docs/roadmap/ChangeSpecs.md`'s P1-2 file list — found by grepping for the same pattern across the whole
solution, not just the files the roadmap doc named). All three depend on `IConnector` and
`PostgreSqlConnector` currently sharing an assembly. Once `IConnector` moves to
`TheGrid.Connectors.Abstractions`, this anchor would resolve to an assembly with **zero** concrete
connector types in it, and every one of these three code paths would silently stop finding any
connector — discovery reports none, execution can't instantiate one by `ConnectorId`, connection
creation can't resolve secret keys. This is the single highest-risk part of an otherwise mechanical
project split.

## Goals / Non-Goals

**Goals:**
- Connector authors can reference a `Mapster`-only, `Npgsql`-free `TheGrid.Connectors.Abstractions`
  package to build a connector without inheriting this project's own driver dependencies.
- Zero behavior change: after this change, connector discovery finds the same 2 connectors
  (`PostgreSqlConnector`, `TestConnector`), `QueryExecutor` resolves and executes them identically, and
  `ConnectionsController` resolves secret keys identically.
- Remove the dead `Dapper` reference while touching this project's dependency list anyway.

**Non-Goals:**
- No `AssemblyLoadContext` isolation or plugin-directory scanning — that's `P1-6`, explicitly sequenced
  after this change and after 2-3 real connectors exist to validate the abstraction (per
  `docs/roadmap/ChangeSpecs.md`'s Phase 2 note).
- No new `TheGrid.Tests.Connectors.Abstractions` test project — see Decisions.
- No change to `QueryResult`/`ConnectorRow`/`QueryResultColumn` (they stay in `TheGrid.Shared.Models`,
  used directly by the client too — moving them would be a much larger, unrelated blast radius for no
  benefit this change needs).

## Decisions

**Namespaces stay exactly as they are (`TheGrid.Connectors`, `TheGrid.Connectors.Attributes`,
`TheGrid.Connectors.Models`) even though the physical project/assembly for most of them becomes
`TheGrid.Connectors.Abstractions`.**
A project's assembly name and its types' namespaces are independent in .NET — nothing requires them to
match. Renaming namespaces to `TheGrid.Connectors.Abstractions.*` would touch every `using` statement and
qualified reference across `TheGrid.Services`, `TheGrid.Server`, `TheGrid.Client`, and every test project
that mentions `IConnector`, `ConnectorAttribute`, `ISchemaDiscovery`, etc. — a large, purely-cosmetic diff
for a change whose own acceptance criteria (per `ChangeSpecs.md`) are "solution builds; tests pass; no
behavior change yet." Keeping namespaces unchanged means the *only* required call-site changes are the
three anchor-type fixes below (plus `.csproj` reference wiring) — everything else that says `using
TheGrid.Connectors;` keeps compiling untouched, whichever assembly the compiler resolves it from.

**The three `Assembly.GetAssembly(typeof(IConnector))` call sites switch their anchor to
`typeof(PostgreSqlConnector)`.**
Considered introducing a dedicated empty marker class (the pattern this codebase already uses in
`TheGrid.Postgres/Assembly.cs` and `TheGrid.Sqlite/Assembly.cs` for EF migrations assemblies) — rejected
for *this* change specifically because naming it `Assembly` in namespace `TheGrid.Connectors` would
collide with `System.Reflection.Assembly` at every call site that needs `using System.Reflection;`
*and* has `TheGrid.Connectors` in scope (all three call sites do), forcing awkward fully-qualified
references. A differently-named marker (e.g. `ConnectorsAssemblyMarker`) would avoid the collision but
adds a new public type whose only job is being a `typeof()` target — for a single concrete project with
exactly two real connectors, anchoring directly on `typeof(PostgreSqlConnector)` (already a real,
necessarily-present type) is simpler and equally correct. This is called out as temporary/scoped: `P1-6`
(plugin/multi-assembly discovery) will replace this whole "one assembly, one anchor type" mechanism with
scanning a configured set of assemblies, at which point a single hardcoded anchor type stops making sense
anyway — no point over-designing the interim mechanism.

**`Extensions/ConnectorExtensions.cs` and `Extensions/TypeExtensions.cs` move to Abstractions, bringing
`Mapster` with them.**
`ConnectorExtensions.GetConnectorParameterDefinitions()` is called from `ConnectorBase.ValidateParameters()`
— since `ConnectorBase` moves to Abstractions (it's dependency-light, no Npgsql/EF), the extension method
it calls must move with it, or `ConnectorBase` (base infrastructure) would depend back on the concrete
project, inverting the intended dependency direction. `TypeExtensions.GetQueryResultColumnTypeForType()`
has no dependency issue itself, but conceptually belongs with the other connector-authoring helpers new
connector authors need (as `PostgreSqlConnector.GetColumns()` demonstrates by using it) — it moves too.
`Mapster` is a lightweight, general-purpose mapping library (not a driver or ORM) — acceptable for
Abstractions per the roadmap's "no Npgsql, no EF, no services" bar, which doesn't exclude it.

**No new `TheGrid.Tests.Connectors.Abstractions` project — the existing `TheGrid.Tests.Connectors`
project references both `TheGrid.Connectors` and `TheGrid.Connectors.Abstractions` and keeps all tests
in one place.**
`CLAUDE.md` documents a convention of test projects mirroring source projects 1:1, which would suggest a
new test project here. Rejected for now: the tests that would move (attribute validation, type-mapper
tests) are small, already exist in `TheGrid.Tests.Connectors`, and splitting them buys nothing until
there's a second or third concrete connector project that would actually want to *not* reference the
first one's test project. Revisit if/when `P2` (adding a second connector) makes this awkward.

## Risks / Trade-offs

- **[Risk]** Missing one of the three anchor-type call sites (or a fourth one this grounding pass
  missed) leaves connector discovery/execution/creation silently broken — no compile error, since
  `Assembly.GetAssembly(typeof(IConnector))` still compiles fine after the split, it just resolves to
  the wrong (connector-free) assembly at runtime.
  **[Mitigation]** tasks.md requires: (a) a test asserting `ConnectorDiscoveryService.RefreshConnectorsAsync()`
  still finds both `PostgreSqlConnector` and `TestConnector` after the split; (b) a test asserting
  `QueryExecutor` can still resolve and instantiate `PostgreSqlConnector` by its `ConnectorId`; (c) a
  grep across the *entire* solution for `GetAssembly(typeof(IConnector))` after the fix, to catch any
  fourth call site this design didn't find; (d) the Docker-based manual check confirms connectors
  actually appear via `GET /api/v1/Connectors` on the running app, which is the most end-to-end proof
  discovery still works.
- **[Risk]** Removing the unused `Dapper` package reference could theoretically break something if it
  was providing an indirect transitive dependency something else relies on.
  **[Mitigation]** Low risk (grepped zero usages in `TheGrid.Connectors` itself), but tasks.md requires a
  full solution build + test run after removing it, not just a build of the one project.

## Migration Plan

No EF migration, no data changes. Purely a compile-time project restructuring. Rollback is a straight
revert of the commit if anything's found broken post-merge — no runtime state to reconcile.

## Open Questions

None — the anchor-type risk and namespace decision were the two open questions, both resolved above
during design.
