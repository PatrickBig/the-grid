# Change Specs (OpenSpec-ready items)

_Purpose: each item below is scoped tightly enough to become a single OpenSpec change proposal and be
implemented by a focused (possibly cheaper) coding model. Each has: **problem → scope → acceptance
criteria → files → gotchas.** Read `docs/architecture/CurrentState.md` first — it has the verified
current contracts these specs build on. Do Phase 0 before Phase 1; do the SDK-contract items (P1) in
order because each depends on the previous._

**Note (2026-07-25):** the Phase 0 ("safe/non-breaking") vs Phase 1 ("breaking SDK contract") split
was written under a production-safety assumption that doesn't apply — this app has no production
deployment or real customer data yet, so breaking the connector contract is free at any point. The
phase numbers below are still useful as a *dependency/sequencing* guide (do X before Y so you don't
redo work), but stop treating "breaking" as a reason to defer or stage something. When a Phase 0 item
and a later Phase 1 item solve the same underlying problem, prefer going straight to the Phase 1 shape
once — see P0-2 below for the first case of this.

**Status (2026-07-26): Phase 0 and Phase 1 are both complete.** P0-4, P0-5, P1-1, P1-2, P1-3, and P1-5
were implemented as sequenced, independent OpenSpec changes (proposed → implemented → verified →
archived → committed one at a time) on 2026-07-26; see each item's status line below and
`openspec/changes/archive/` for the archived proposals. P1-6 is intentionally deferred to Phase 2 (it
needs 2-3 real connectors first to prove the abstraction, per its own gotcha note). Next up is Phase 2
— see the bottom of this file — not yet started or scoped as of 2026-07-26.

Separately, four pre-existing bugs unrelated to any roadmap item were found during manual end-to-end
verification of this work and fixed directly (not via OpenSpec, since they weren't roadmap-scoped):
a `SetupHostedService` singleton/scoped-DI captive dependency, a `GroupManager.CreateGroupAsync` EF
change-tracking bug that broke new-organization creation, a Hangfire `[Queue]` attribute that was
silently inert (interface vs. implementation reflection mismatch), and a `ConnectorDiscoveryService`
rerun crash. All fixed and verified 2026-07-26.

Legend: 🔴 critical · 🟠 high · 🟡 medium. Estimates are rough size, not time.

---

## Phase 0 — Correctness & safety (independent, ship in any order)

### P0-1 ✅ DONE — Encrypt connection secrets
**Status (2026-07-25).** Already implemented on `feature/group-and-permission-manager`:
`TheGrid.Services/Security/ISecretProtector.cs` + `AesGcmSecretProtector.cs` (AES-GCM, tested in
`AesGcmSecretProtectorTests.cs`), wired into `QueryExecutor.GetConnector` which decrypts
`Connection.SecretProperties` at read time. Took the interim path noted below (no `IsSecret` on
`ConnectorParameterAttribute` yet — P1-1 still not done). Original spec kept below for reference/audit.

**Problem.** `Connection.ConnectionProperties` is stored as plaintext JSON; the property's XML doc
falsely claims encryption. (CurrentState → Persistence & secrets.)
**Scope.** Encrypt only secret-flagged params at rest; redact them in API responses. Depends on
P1-1 (the `IsSecret` flag) for *per-field* precision — if P1-1 isn't done yet, do the interim
version below.
**Acceptance criteria.**
- New `ISecretProtector` abstraction in `TheGrid.Services` (or `TheGrid.Data`), default impl using
  **AES-GCM** with a key sourced from configuration (`SystemOptions`), storing a key id + nonce
  alongside ciphertext to allow rotation.
- Secret values are ciphertext at rest (verify via a raw DB read in a test).
- API responses never return secret plaintext — return a sentinel (`hasValue: true` / `"********"`);
  a secret is only overwritten when the client sends a new non-sentinel value.
- The false XML doc comment on `Connection.ConnectionProperties` is corrected to match reality.
- **Do not** add encryption to the shared `JsonColumnConverter<T>` (it's reused by non-secret columns).
- Migration added for both providers if column shape changes.
**Files.** `TheGrid.Models/Connection.cs`, `TheGrid.Data/TheGridDbContext.cs`, new protector class,
connection controller/DTO in `TheGrid.Server`, `SystemOptions.cs`. Migrations in `TheGrid.Postgres` + `TheGrid.Sqlite`.
**Gotchas.** Interim (pre-P1-1): treat any param whose type is `ProtectedText` as secret. Key
management: document that losing the key = losing all stored connections; support key rotation via the stored key id.
**Size.** M.

### P0-2 ✅ Streaming execution with MaxRows, timeout, and batched inserts — Done
**Status (2026-07-25, done).** Implemented via the `streaming-query-execution-limits` OpenSpec change.
Revised scope — this now supersedes the original P0-2 and P0-3 below, and pulls forward P1-4
(streaming). Rationale: this app isn't in production, so the connector contract can be broken freely;
doing the "cheap" post-hoc-truncation version first and the real streaming version later (as originally
staged) would mean touching `PostgreSqlConnector`'s read loop twice for the same problem. Going straight
to the target shape once — see `docs/architecture/CurrentState.md`'s "Query execution path" and
"Connector SDK" sections for the as-built contract.

**Problem.** `QueryExecutor` buffers the entire result set via `IConnector.GetDataAsync` — which
itself fully buffers into a `List<Dictionary<string,object?>>` inside `PostgreSqlConnector` — before
any row reaches the DB. No row cap, no timeout, one unbounded `SaveChanges`. A big query can OOM the
agent, and the original "MaxRows enforced by the executor" framing was never actually achievable
without a connector-side change, since the executor never sees a row until the connector has already
buffered all of them.
**Scope.**
- Replace `GetDataAsync`'s role as the primary execution path with a streaming shape
  (`IAsyncEnumerable<QueryResultRow>` or equivalent) that `PostgreSqlConnector` implements via its
  existing reader loop (`yield return` per row instead of accumulating into `rows`).
- `QueryExecutor` enumerates and stops at `MaxRows` (sets a new `Truncated` bool on `QueryExecution`),
  batches inserts (`SaveChanges` every N rows, e.g. 500, clearing the change tracker) instead of
  accumulating everything, and wraps enumeration in a linked `CancellationTokenSource` for the timeout
  (records a new `TimedOut` status on `QueryExecutionStatus`, alongside `Error`).
- `ExecutionLimits` (MaxRows, Timeout) sourced from `SystemOptions`, with optional per-query override.
**Files.** New streaming interface/method in `TheGrid.Connectors`, `PostgreSqlConnector.cs`,
`TestConnector.cs` (needs a streaming impl + a way to fake a large row count for testing
`MaxRows`/`Truncated`), `TheGrid.Services/QueryExecutor.cs`, `SystemOptions.cs`,
`TheGrid.Models/QueryExecution.cs`, `TheGrid.Shared/Models/QueryExecutionStatus.cs`. Migrations in
both `TheGrid.Postgres` and `TheGrid.Sqlite` (new columns/enum value).
**Gotchas.** Column metadata is currently discovered from the first row inside `GetDataAsync` —
preserve that behavior on the first yielded row when converting to streaming.
**Open question.** Whether to also fold in P1-1 (`Key`/`IsSecret` on connector parameters) here since
the connector contract is already being broken in this change — undecided; only pull it in if it stays
small once the streaming work is underway.
**Size.** M–L (was S–M + S + M split across three separate items).

### P0-3 — Superseded, folded into P0-2 above — Done (via P0-2)
Batched inserts are now part of P0-2's scope (streaming + batching are the same mechanism), implemented
and done alongside it. Left here for traceability only.

### P0-4 ✅ DONE — Fix type mapper bug + expand types
**Status (2026-07-26).** Implemented via the `fix-type-mapper` OpenSpec change (archived
`2026-07-25-fix-type-mapper`). Original spec kept below for reference.

**Problem.** `GetQueryResultColumnTypeForType` has a duplicate `long` branch and unreachable `uint` arm; no `Guid`/binary/JSON.
**Acceptance criteria.**
- Rewrite as an explicit map; `uint`→`Long`, `long`/`ulong` correct, add `Guid`, `byte[]`, JSON handling.
- Extend `QueryResultColumnType` enum with the new types (+ `Unknown`).
- Unit tests covering each mapped CLR type.
**Files.** `TheGrid.Connectors/Extensions/TypeExtensions.cs`, `TheGrid.Shared/Models/QueryResultColumn.cs`, tests in `TheGrid.Tests.Connectors`.
**Gotchas.** Adding enum values may touch the Mapster `Adapt` between `Shared` and `Models` column-type enums (`QueryExecutor.UpdateColumnDefinitions`) — keep both enums in sync.
**Size.** S.

### P0-5 ✅ DONE — Rewrite `Creating-Connectors.md`
**Status (2026-07-26).** Implemented via the `rewrite-connector-docs` OpenSpec change (archived
`2026-07-25-rewrite-connector-docs`). Original spec kept below for reference.

**Problem.** Doc references `QueryRunner`/`QueryRunnerBase`/`RunQueryAsync` — none exist. It's unfollowable.
**Acceptance criteria.** Doc uses real names (`Connector`/`ConnectorBase`/`GetDataAsync`, `[Connector]`, `[ConnectorParameter]`); `PostgreSqlConnector` is the worked example; capability interfaces documented. If P1 items land, update to the new contract.
**Files.** `docs/Creating-Connectors.md`.
**Size.** S.

---

## Phase 1 — SDK contract (BREAKING; do in order, before adding connectors)

### P1-1 ✅ DONE — Add stable `Key` + `IsSecret` + rich parameter metadata
**Status (2026-07-26).** Implemented via the `connector-parameter-keys` OpenSpec change (archived
`2026-07-25-connector-parameter-keys`). Scoped down from the original spec: added only `Key` and
`IsSecret`; `Group`/`DefaultValue`/`Placeholder`/`VisibleWhen`/`Validation` were explicitly deferred
(no current consumer). No data migration script was added — this app has no production data, so
existing local connections need manual recreation, an accepted consequence. Original spec kept below
for reference.

**Problem.** Params are keyed by display name; no machine id; no secret flag; no validation metadata. (CurrentState trap #1, #2.)
**Acceptance criteria.**
- `ConnectorParameterAttribute` and `ConnectionProperty` gain: `Key` (immutable machine id, required),
  `IsSecret`, and optionally `Group`, `DefaultValue`, `Placeholder`, `VisibleWhen`, `Validation`.
- `ProtectedText` implies `IsSecret = true`.
- Stored `ConnectionProperties` keyed by `Key`, not display `Name`. **Provide a data migration** mapping
  existing display-name keys → new keys for shipped connectors (Postgres, Test).
- `ConnectorBase.ValidateParameters` and `PostgreSqlConnector` read by `Key`.
**Files.** `TheGrid.Connectors/Attributes/ConnectorParameterAttribute.cs`, `CommonConnectionParameters.cs`
(add `Key`s), `TheGrid.Shared/Models/ConnectionProperty.cs`, `PostgreSqlConnector.cs`, `ConnectorBase.cs`,
`ConnectorDiscoveryService.cs` (Mapster mapping), data migration both providers.
**Gotchas.** This is the linchpin for P0-1's per-field encryption — sequence accordingly. Keep `Name`
for display; never reuse it as a key again.
**Size.** L.

### P1-2 ✅ DONE — Split `TheGrid.Connectors.Abstractions`
**Status (2026-07-26).** Implemented via the `connectors-abstractions-split` OpenSpec change (archived
`2026-07-25-connectors-abstractions-split`). Namespaces deliberately left unchanged (only the assembly
moved) to keep the diff small. Found and fixed a correctness-critical issue not in the original scope:
three call sites (`ConnectorDiscoveryService`, `QueryExecutor`, `ConnectionsController`) resolved "the
connectors assembly" via `Assembly.GetAssembly(typeof(IConnector))`, which would've silently broken
once `IConnector` moved out of the connectors assembly — all three now anchor on
`typeof(PostgreSqlConnector)` instead. `QueryResultColumn`/`QueryResult` stayed in `TheGrid.Shared` as
the gotcha note anticipated. Original spec kept below for reference.

**Problem.** Connector authors transitively depend on Npgsql/EF/services; discovery is single-assembly. (CurrentState trap #3.)
**Acceptance criteria.**
- New `TheGrid.Connectors.Abstractions` project holding only interfaces, attributes, and `Models/`
  (no Npgsql, no EF, no services).
- `TheGrid.Connectors` (concrete) references it; core references abstractions.
- Solution builds; tests pass; no behavior change yet.
**Files.** New project + `.csproj`; move `IConnector`, `ConnectorBase`, attributes, `Models/`, capability
interfaces. Update references in `TheGrid.Services`, `TheGrid.Shared` usage.
**Gotchas.** `QueryResult`/`QueryResultColumn` currently live in `TheGrid.Shared.Models` — decide whether
they move to Abstractions or stay shared (they're used by client too; likely keep in Shared and reference it).
**Size.** M.

### P1-3 ✅ DONE — `IConnectorFactory` + `ConnectorContext` (kill raw reflection)
**Status (2026-07-26).** Implemented via the `connector-factory` OpenSpec change (archived
`2026-07-26-connector-factory`). Deliberately excluded `ExecutionLimits` from `ConnectorContext` —
would force `TheGrid.Connectors.Abstractions` to depend on `TheGrid.Models` (which pulls in EF Core)
for a value no connector actually consults. Found and fixed a real gap: `IHttpClientFactory` had no
server-side DI registration anywhere before this (`AddHttpClient()` was only ever called client-side).
`ConnectorId == type.FullName` semantics preserved. Original spec kept below for reference.

**Problem.** `Activator.CreateInstance` blocks DI — no logger, no `IHttpClientFactory`, no secret resolver. (CurrentState trap #4.)
**Acceptance criteria.**
- `ConnectorContext` record (params, `ILoggerFactory`, `IHttpClientFactory`, `ExecutionLimits`).
- `IConnectorFactory.Create(connectorId, context)` implemented in the host; `QueryExecutor` uses it
  instead of `Activator`.
- `ConnectorBase` ctor accepts the context (or params + context); existing connectors updated.
- Registered in DI.
**Files.** `TheGrid.Services/QueryExecutor.cs`, new factory in `TheGrid.Services`, `ConnectorBase.cs`,
`PostgreSqlConnector.cs`/`TestConnector.cs`, DI registration (`TheGridContextServices.cs`).
**Gotchas.** Preserve `ConnectorId == type.FullName` semantics so existing `Connection` rows resolve.
Depends conceptually on P0-2 (`ExecutionLimits`).
**Size.** M.

### P1-4 — Superseded, pulled forward into P0-2 above — Done (via P0-2)
**Status (2026-07-25, done).** No longer a separate later-phase item — folded into the revised P0-2,
done now rather than staged, since breaking the connector contract is free pre-production. Original spec
kept for reference; the implemented shape resolved the open question below: `GetDataAsync` did **not**
survive as a buffered fallback — it was replaced outright with the streaming signature, no dual-path
capability interface.

**Problem.** `GetDataAsync` fully buffers. (Roadmap §3.5.)
**Acceptance criteria.**
- `IStreamingConnector.StreamDataAsync(...) : IAsyncEnumerable<QueryResultRow>`.
- `QueryExecutor` prefers streaming when the connector implements it, batching inserts (P0-3) as it goes.
- `PostgreSqlConnector` implements it via the existing reader loop.
- `GetDataAsync` remains the default for non-streaming connectors.
**Files.** New interface in Abstractions, `PostgreSqlConnector.cs`, `QueryExecutor.cs`.
**Gotchas.** Column metadata is discovered on first row today — preserve that when streaming.
**Size.** M.

### P1-5 ✅ DONE — Enrich capability results
**Status (2026-07-26).** Implemented via the `enrich-capability-results` OpenSpec change (archived
`2026-07-26-enrich-capability-results`). Also fixed a real pre-existing bug found along the way:
`PostgreSqlConnector.TestConnectionAsync` never actually executed its test query and let failures
throw instead of reporting them. Renamed `IPermissionTest`/`HasWritePermissionAsync` →
`IWriteAccessProbe`/`HasWriteAccessAsync`; added `Connector.SupportsWriteAccessProbe` (new EF migration
both providers) matching the existing `SupportsConnectionTest`/`SupportsSchemaDiscovery` pattern.
Deliberately did **not** build a controller/UI for connection testing — nothing calls these methods
outside the connector's own tests yet, so that's separate future feature work, not part of this item.

- `IConnectionTest` → return `ConnectionTestResult(bool Success, string? Message, TimeSpan Elapsed)` instead of `bool`.
- Rename `IPermissionTest`/`HasWritePermissionAsync` → `IWriteAccessProbe` (name reads as app-perms today);
  surface result as a UI warning badge.
**Files.** `IConnectionTest.cs`, `IPermissionTest.cs`, `PostgreSqlConnector.cs`, discovery flags in
`ConnectorDiscoveryService.cs`, connection controller + client display.
**Gotchas.** Update `Connector.SupportsConnectionTest` plumbing. Breaking rename — grep all references.
**Size.** S–M.

### P1-6 🟢 Plugin/multi-assembly discovery — deferred to Phase 2
**Status (2026-07-26).** The only Phase 0/1 item not yet done — intentionally deferred per its own
gotcha note below (needs Abstractions, done in P1-2, plus 2-3 real connectors to prove the plugin
shape against before committing to it). Pick this up as part of Phase 2.

**Problem.** Only the `IConnector`-defining assembly is scanned. (Roadmap §3.3.)
**Acceptance criteria.** Discovery scans a configured set of assemblies / a plugin directory; connectors
in separate assemblies register; consider `AssemblyLoadContext` isolation for driver-version conflicts.
**Files.** `ConnectorDiscoveryService.cs`, `SystemOptions.cs`.
**Gotchas.** Do this last — after Abstractions (P1-2) exists so plugins have a thin package to reference.
**Size.** M–L.

---

## Phase 2+ — validate & build (specs to be expanded later)
- **P2:** add MySQL/SQL Server (2nd SQL engine), a REST/HTTP source (exercises `IHttpClientFactory`),
  and a file/columnar source — proves the SDK. Then P1-6 plugin loading.
- **P3:** query parameters (`IParameterizedQuery`, safe binding — never string-concat SQL), result
  caching, CSV/Excel export, then **Dashboards** (new `Dashboard` aggregate + widget layout + builder UI).
- **P4:** alerts, audit log, API tokens, scoped share links, retention/cleanup jobs.

Expand each into its own spec when you reach it; the Phase 3 Dashboard work is large enough to be
several changes on its own.
