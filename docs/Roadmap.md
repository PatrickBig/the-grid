# The Grid — Roadmap & Architecture Review

_Last updated: 2026-07-23. Author: architecture review pass after project dormancy._

> **Progress note (2026-07-26):** Phase 0 and Phase 1 below are both complete — see
> `docs/roadmap/ChangeSpecs.md` for per-item status and what shipped for each. Next up is Phase 2
> (§7). This file's narrative/analysis content is unchanged and still accurate; only ChangeSpecs.md
> tracks live status.

This document evaluates the current state of The Grid, identifies the gap between where it is
and the stated goal — **a more secure, more stable, more enterprise-friendly Redash** — and lays
out a prioritized roadmap. The **Connectors SDK gets the deepest treatment** because that is the
extensibility surface the whole product's value scales on.

Added two docs tailored to your OpenSpec + cheaper-model workflow:

docs/architecture/CurrentState.md — verified ground truth so the executing model doesn't hallucinate contracts. Tables of every connector type with real signatures and file paths, the query-execution path step-by-step, the auth model, and an explicit "traps" list (display-name-as-key, no secret flag, single-assembly discovery, raw-reflection instantiation, the type-mapper bug). It ends with the "if code and this doc disagree, code wins" rule so it doesn't rot into a liability.

docs/roadmap/ChangeSpecs.md — each roadmap item pre-decomposed into an OpenSpec-ready change with problem → scope → acceptance criteria → files → gotchas, sized S/M/L. Phase 0 items are independent; Phase 1 (the breaking SDK-contract changes) are ordered with dependencies called out — e.g. P1-1 (Key + IsSecret) is flagged as the linchpin that P0-1's per-field encryption depends on.

Two things I deliberately baked into the specs to protect a cheaper model from predictable mistakes:
- P0-1 warns not to add encryption to the shared JsonColumnConverter<T> — it's reused by non-secret columns, so the naive fix would encrypt the wrong things.
- P0-4 flags the Mapster enum sync between Shared and Models column-type enums, which a model touching only TypeExtensions.cs would miss.

A few notes on what I'd recommend for the OpenSpec pass itself:
- Feed CurrentState.md as context to every change — it's the anti-hallucination anchor. Cheap.
- Keep P1-1 and P1-2 on the more capable model if you can. The Abstractions split and the display-name→Key data migration are the two places where a wrong move creates silent data loss across existing connections; the rest are safe for a cheaper model.
- P0 items are genuinely independent — good parallelization candidates.

Everything's written to the repo, nothing committed. Want me to leave it there, or would a short index entry in the docs README / a top-level pointer help you navigate the three files (Roadmap.md, architecture/CurrentState.md, roadmap/ChangeSpecs.md)?


---

## 1. Executive summary

The Grid has a **clean, well-factored foundation**: interface-segregated managers, per-provider EF
migrations, a real authorization model (org-scoped groups + fine-grained permissions), background
execution via Hangfire, SignalR for live query updates, and StyleCop discipline throughout. The
bones are good and distinctly more "enterprise" in shape than Redash's.

The gaps that stand between it and the goal fall into three buckets:

| Bucket | Severity | Summary |
|---|---|---|
| **Security correctness** | 🔴 Critical | Connection secrets are stored as **plaintext JSON**, despite the model claiming encryption. This alone blocks the "more secure than Redash" claim. |
| **Connectors SDK** | 🟠 High | Works, but the current shape (stringly-typed dictionaries keyed by display name, in-process reflection-only discovery, fully-buffered results, no DI) will not scale to "many data sources, easily." |
| **Feature completeness** | 🟡 Medium | No dashboards, alerts, query parameters, result caching, or export — all core to a Redash replacement. The permission enum already anticipates these; the implementations don't exist yet. |

The roadmap below sequences these: **stop the security bleeding first**, then **re-lay the connector
SDK foundation** (cheaper to change now than after 15 connectors exist), then **build out features**
on top of the stable base.

---

## 2. Current state assessment

### What's working well — keep doing this

- **Interface-per-manager discipline** (`IGroupManager`, `IQueryExecutor`, …). Clean DI, testable, honest seams.
- **Primary-constructor service style** is consistent and readable.
- **Provider-split migrations** (`TheGrid.Postgres` / `TheGrid.Sqlite` as migrations-only assemblies) is the right call and rare to see done cleanly.
- **Authorization model** is genuinely thought-through: org-scoping via header-vs-claim (`OrganizationHandler`) plus resource-based handlers (`ConnectionAuthorizationHandler`). This is the differentiator vs Redash's coarse model — lean into it.
- **Capability interfaces on connectors** (`ISchemaDiscovery`, `IConnectionTest`, `IPermissionTest`) — opt-in capabilities is exactly the right pattern (see §3, where I argue for *more* of them).
- **`RunMode`** (Server/Agent/Mixed) already contemplates horizontal scale-out.

### Notable correctness issues found during review

1. **🔴 Secrets are not encrypted.** `Connection.ConnectionProperties` XML doc says _"This value is
   encrypted in the database when stored,"_ but `TheGridDbContext` maps it with
   `JsonColumnConverter<>`, which is plain `System.Text.Json`. Passwords and connection strings are
   at rest in cleartext. This is the single most important thing to fix. (§4.1)

2. **🟠 `QueryExecutor` buffers entire result sets in memory**, then inserts one `QueryResultRow` per
   row with no batching and no row cap. A large query can OOM the agent process. (§3.5, §4.3)

3. **🟡 `TypeExtensions.GetQueryResultColumnTypeForType` has dead/duplicate branches** — `long` is
   handled twice and `uint` falls into an unreachable arm. Minor now, but it's the kind of thing that
   silently mistypes columns. (§3.4)

4. **🟡 The connector authoring doc (`Creating-Connectors.md`) is stale** — it documents
   `QueryRunner`/`QueryRunnerBase`/`RunQueryAsync`, none of which exist anymore (the code uses
   `Connector`/`ConnectorBase`/`GetDataAsync`). Anyone following it cannot produce a working
   connector. (§3.6)

5. **🟡 `queryParameters` is plumbed as `null` everywhere.** `QueryExecutor.RefreshQueryResultsAsync`
   always passes `null`, so parameterized queries — a headline Redash feature — are structurally
   impossible today even though the connector signature supports them. (§5.3)

---

## 3. Connectors SDK — deep review (primary focus)

This is where you asked for the most feedback, so this section is the longest. I'll assess the
current design honestly, then propose a concrete target shape.

### 3.1 How it works today

- A connector is a class in the `TheGrid.Connectors` assembly that inherits `ConnectorBase` and is
  decorated with `[Connector(...)]` + one `[ConnectorParameter(...)]` per connection field.
- Parameters arrive as `Dictionary<string, string>`, **keyed by the human display name**
  (`"Connection String"`, `"Password"`).
- `GetDataAsync(string query, Dictionary<string,object?>? parameters, ct)` returns a `QueryResult`
  holding **all** columns and **all** rows in memory.
- Optional capabilities are opt-in interfaces: `ISchemaDiscovery`, `IConnectionTest`, `IPermissionTest`.
- Discovery (`ConnectorDiscoveryService`) reflects over **only the assembly that defines `IConnector`**
  and persists a `Connector` row per type. Instantiation (`QueryExecutor.GetConnector`) is
  `Activator.CreateInstance(type, connectionProperties)`.

The opt-in capability pattern is the strongest part of the design and should be extended, not
replaced. The problems below are about the *plumbing* around it.

### 3.2 Problem: parameters are stringly-typed and keyed by display name

This is the most consequential design smell. Consequences:

- **Renaming a label is a data-migration event.** The dictionary key _is_ the UI label. Change
  `"Database Name"` to `"Database"` and every stored connection silently loses that value.
- **No stable machine identity.** There's no `Key`/`Id` separate from `Name`, so you can't localize
  labels, can't reorder safely, can't alias.
- **No typing.** Everything is `string`. Numeric/boolean/enum params get parsed ad hoc at each use
  site (see `TestConnector` re-parsing `"NumberOfRows"`).
- **No validation contract beyond `Required`.** No min/max, regex, allowed-values, or
  "this field is a secret" flag — which is exactly the metadata the encryption layer (§4.1) needs.
- **Collides with secret handling.** Because there's no `IsSecret` on the parameter definition, the
  storage layer can't know *which* values to encrypt or redact in API responses.

**Recommendation.** Introduce a stable `Key` (machine id, immutable) distinct from `Name` (display),
and richer parameter metadata. Keep the `Dictionary<string,string>` on the wire if you like, but key
it by `Key`, not `Name`.

```csharp
[ConnectorParameter(
    Key = "connectionString",              // immutable machine id — the storage key
    Name = "Connection String",            // display label, free to change/localize
    Type = ConnectionPropertyType.SingleLineText,
    Required = true,
    IsSecret = false,
    HelpText = "Standard PostgreSQL connection string.")]
[ConnectorParameter(
    Key = "password", Name = "Password",
    Type = ConnectionPropertyType.ProtectedText,
    Required = true, IsSecret = true)]      // <-- drives encryption + API redaction
```

Add to the parameter model: `IsSecret`, `Group` (for visually grouping fields like "Authentication"
vs "Advanced"), `DependsOn`/`VisibleWhen` (show port only when not using a full connection string),
`DefaultValue`, `Placeholder`, and a `Validation` sub-object (regex / min / max / allowed values).
`ProtectedText` should imply `IsSecret = true`.

### 3.3 Problem: connectors aren't actually pluggable

Discovery only scans the single assembly containing `IConnector`. "Extensible into many data
sources" implies third parties (or you) can ship a connector **without recompiling the core**. Today
every connector must live in `TheGrid.Connectors`.

**Recommendation.** Move to an explicit plugin-discovery model:

- Define connectors in **separate assemblies** (`TheGrid.Connectors.Postgres`,
  `TheGrid.Connectors.MySql`, …) that reference a thin **`TheGrid.Connectors.Abstractions`** package
  (just the interfaces, attributes, and models — no Npgsql, no EF).
- Discover by scanning a configured plugin directory / a registered list of assemblies, ideally each
  in its own `AssemblyLoadContext` so a connector's transitive dependencies (a specific driver
  version) don't leak into or conflict with the host.
- Long-term this is what lets the community add connectors as NuGet packages you didn't compile.

The `Abstractions` split is worth doing even before the load-context work — it stops connector
authors from taking an accidental dependency on your whole services graph.

### 3.4 Problem: type mapping is lossy and buggy

`GetQueryResultColumnTypeForType` (in `TheGrid.Connectors/Extensions/TypeExtensions.cs`):

- `long` is tested twice; the second arm is dead. `uint` shares that dead arm and never resolves as
  intended.
- No mapping for `Guid`, `byte[]`/binary, `Guid`, JSON, or geospatial — all collapse to `Text`.
- `QueryResultColumnType` itself is missing `Guid`, `Json`, `Binary`, and an explicit `Date`
  (vs `DateTime`) / `Unknown`.

**Recommendation.** Rewrite the mapper as a dictionary lookup, expand
`QueryResultColumnType`, and add an escape hatch: let a connector return a
**native type name** alongside the normalized enum (you already do this for schema columns via
`DatabaseObjectColumn.TypeName` — mirror it on `QueryResultColumn`). That preserves fidelity
("`numeric(10,2)`", "`jsonb`") for the UI without forcing every type into the enum.

### 3.5 Problem: results are fully buffered — no streaming, paging, or limits

`GetDataAsync` returns `QueryResult` with `List<Dictionary<string,object?>> Rows`. Everything is in
memory before the caller sees row one, and `QueryExecutor` then re-buffers it into EF change-tracking
row by row.

For an enterprise tool this is the scariest scalability cliff. A `SELECT *` against a big table takes
down the agent.

**Recommendation (staged):**

- **Now (cheap):** add a `MaxRows` / `RowLimit` to execution (config + per-query override) and a
  query timeout via the `CancellationToken` you already thread through. Enforce both in the executor.
- **Next:** add a streaming capability interface so large sources don't have to buffer:

  ```csharp
  public interface IStreamingConnector
  {
      IAsyncEnumerable<QueryResultRow> StreamDataAsync(
          QueryRequest request, CancellationToken ct = default);
  }
  ```

  Keep `GetDataAsync` as the simple default; connectors that can stream opt in, and `QueryExecutor`
  batches inserts (e.g. `SaveChanges` every N rows) instead of accumulating everything.

### 3.6 Problem: instantiation is reflection-only — no DI, no shared infrastructure

`Activator.CreateInstance(type, connectionProperties)` means a connector can never receive an
`ILogger`, an `IHttpClientFactory` (essential for HTTP/REST-based sources — BigQuery, Snowflake REST,
any SaaS API), a proxy config, or a secret-resolver. Every connector is on its own for cross-cutting
concerns.

**Recommendation.** Introduce a **connector factory** abstraction that the host implements, so
connectors are constructed with a context object rather than by raw reflection:

```csharp
public sealed record ConnectorContext(
    IReadOnlyDictionary<string, string?> Parameters,
    ILoggerFactory LoggerFactory,
    IHttpClientFactory HttpClientFactory,
    ExecutionLimits Limits);        // MaxRows, Timeout, etc.

public interface IConnectorFactory
{
    IConnector Create(string connectorId, ConnectorContext context);
}
```

This also gives you the seam to cache/pool connectors and to inject a **secret resolver** (so a
parameter value can be a reference like `vault://…` rather than the literal secret — see §4.1).

### 3.7 Problem: capability results are too thin for good UX

- `IConnectionTest.TestConnectionAsync` returns `bool`. When it fails the user gets "it failed" with
  no reason. Return a result object: `record ConnectionTestResult(bool Success, string? Message,
  TimeSpan Elapsed)`.
- `IPermissionTest` is a genuinely nice safety idea (warn if the connection can write) but the name
  reads like app-permissions. Consider `IWriteAccessProbe` / `SupportsReadOnlyVerification` and
  surface the result as a **warning badge** on the connection ("This account can modify data").

### 3.8 Suggested capability catalog (the extensible surface)

The opt-in-interface pattern is the right backbone. A fuller catalog to grow into:

| Interface | Purpose | Status |
|---|---|---|
| `IConnector` (core) | Run a query, return results | ✅ exists |
| `ISchemaDiscovery` | Enumerate tables/columns for the browser + autocomplete | ✅ exists |
| `IConnectionTest` | Validate credentials (→ return a result, not bool) | ✅ exists, tweak |
| `IWriteAccessProbe` (was `IPermissionTest`) | Warn if connection is not read-only | ✅ exists, rename |
| `IStreamingConnector` | `IAsyncEnumerable` results for large sets | ➕ new (§3.5) |
| `IParameterizedQuery` | Declares parameter-binding style so the host can bind safely | ➕ new (§5.3) |
| `IQueryCanceler` | Kill an in-flight query server-side on cancel | ➕ new |
| `ICostEstimator` | Dry-run / EXPLAIN for "this will scan 4TB" guardrails | ➕ new (nice-to-have) |
| `ISchemaAutocomplete` | Lightweight symbol feed for the Monaco editor | ➕ new |

Each is small, testable, and independently shippable — which is exactly the "powerful SDK, add
sources easily" property you want.

### 3.9 Connector SDK — recommended sequence

1. Split **`TheGrid.Connectors.Abstractions`** out (interfaces + attributes + models only).
2. Add **`Key` + `IsSecret`** (and the richer parameter metadata) to `ConnectorParameterAttribute`
   and `ConnectionProperty`; migrate storage keys from display-name to `Key`.
3. Introduce **`IConnectorFactory` + `ConnectorContext`**; delete the raw `Activator` call.
4. Add **`ExecutionLimits`** (MaxRows + Timeout) and enforce in `QueryExecutor`.
5. Add **`IStreamingConnector`** + batched inserts.
6. Rewrite the **type mapper**; expand `QueryResultColumnType`; carry native type names.
7. Move discovery to **multi-assembly / plugin** loading.
8. **Rewrite `Creating-Connectors.md`** to match reality, with the PostgreSQL connector as the worked example.

Do 1–4 before writing any new connectors — they're breaking changes to the SDK contract and get more
expensive with every connector that exists. 5–8 can follow incrementally.

---

## 4. Security & stability hardening

### 4.1 Encrypt connection secrets (🔴 do this first)

Right now `ConnectionProperties` is plaintext JSON. Target design:

- A pluggable **`ISecretProtector`** with an envelope-encryption default (AES-GCM with a key from
  configuration / a KMS / DPAPI), and room for a HashiCorp Vault or cloud-KMS backend.
- Encrypt **only** parameters flagged `IsSecret` (§3.2). Non-secret params stay queryable/plaintext.
- **Never** return secret values to the client. API responses should send back a sentinel
  (`"********"` / `hasValue: true`) and only overwrite on an explicit change.
- Rotate-ability: store a key id alongside the ciphertext so you can re-wrap on key rotation.

Also fix the now-false XML doc comment on `Connection.ConnectionProperties` once this is real.

### 4.2 Secrets in transit & audit

- Add an **audit log** (who created/modified/executed what, when, from where). Enterprise buyers ask
  for this on day one; the permission model already gives you the actor context.
- Confirm connection-test / schema-discovery endpoints enforce the same resource authorization as
  execution (a read of `ConnectionAuthorizationHandler` coverage across all connection endpoints).

### 4.3 Execution safety

- Enforce **MaxRows** and **query timeout** (§3.5) — protects both the agent and the source DB.
- **Batch** `QueryResultRow` inserts; don't accumulate a whole result set in the EF change tracker.
- Add a **result-retention / cleanup** job (old `QueryExecution` + `QueryResultRow` rows grow unbounded).
- Consider a **circuit breaker / concurrency cap per connection** so one runaway dashboard can't
  saturate a source.

---

## 5. Feature roadmap (toward Redash parity, done better)

The permission enum already names the entities that don't exist yet (`Dashboard`, `Alert`,
`Folder`). That's a good signal for sequencing.

### 5.1 Dashboards 🔴 (biggest missing pillar)

There is no `Dashboard` entity at all — only queries + a single table/chart visualization. A
dashboarding tool needs: a `Dashboard` aggregate, widget placement (grid layout), per-widget query
binding, dashboard-level parameters/filters, and refresh coordination. This is the largest single
feature gap between here and "Redash replacement."

### 5.2 Visualizations

- Only `TableVisualizationManager` is real; `ChartVisualization` is a stub. Build out the chart types
  (line, bar, area, pie, scatter, single-stat/counter, pivot). The `VisualizationManagerFactory`
  pattern is already the right extension seam — mirror the connector capability approach.
- **Hand this to Claude Design** (see §6) but the *manager/data* contracts are yours to define first.

### 5.3 Query parameters / variables 🟠

The connector signature already accepts `queryParameters` but nothing supplies them. Build:
parameter definitions on a `Query` (name, type, default, allowed values / dropdown-from-query), safe
binding per connector (`IParameterizedQuery`, §3.8 — **never** string-concatenate into SQL), and UI
inputs. This is both a feature and a SQL-injection safety boundary.

### 5.4 Alerts 🟡

`CreateAlert`/`ModifyAlert` permissions exist; the feature doesn't. Threshold rules over query
results, evaluation on the Hangfire schedule you already run, notification channels (email is already
wired via `Emailer`/`EmailSendJob`; add webhook/Slack later).

### 5.5 Supporting features

- **Result caching** keyed by (query, params, connection) with TTL — big perceived-perf win and load reducer.
- **Export** results to CSV/Excel/JSON.
- **Scheduling UI** for `QueryRefreshManager` (backend exists; expose it).
- **Folders / tagging / search** (`ManageFolders` permission is defined; `ITags` exists on queries — extend).
- **API tokens** for programmatic access + embedding.
- **Public / shared links** with scoped read-only access (an enterprise-safe version of Redash's public dashboards).

---

## 6. UI / UX (for the Claude Design pass)

Stack is Blazor WASM + **Radzen** + **BlazorMonaco** (good editor choice). Notes to feed into the
design work:

- **Connection creation is dynamic-form-driven** (`ConnectionPropertyEditor.razor` renders from
  `ConnectionProperty` metadata). Every enrichment in §3.2 (grouping, `VisibleWhen`, validation,
  placeholders, secret masking) pays off directly here — the richer the parameter metadata, the
  better this screen gets for free. **This is the highest-leverage UI/SDK coupling in the app.**
- **Schema browser + editor autocomplete**: wire `ISchemaDiscovery` output into Monaco completion.
- **Query results grid** needs virtualized/paged rendering to match the streaming/limit work (§3.5).
- **Dashboard builder** (§5.1) is net-new UI — the big design lift.
- **Connection test / write-access feedback**: surface §3.7's richer results as inline status +
  warning badges rather than a bare success/fail toast.
- General: an onboarding/setup flow for the permission-model choice (`docs/Permissions.md` describes
  it; the UI should make it a guided first-run step).

---

## 7. Suggested phasing

**Phase 0 — Stop the bleeding (security correctness).**
Encrypt secrets (§4.1), add MaxRows + timeout + batched inserts (§4.3), fix the type-mapper bug
(§3.4), correct/rewrite the connector doc (§3.6). Small, high-value, unblocks the "secure/stable" claim.

**Phase 1 — Re-lay the connector SDK foundation.**
Abstractions split, `Key`/`IsSecret` + rich parameter metadata, `IConnectorFactory`/`ConnectorContext`,
streaming interface. Breaking changes — do them before more connectors exist. (§3.9 steps 1–5.)

**Phase 2 — Prove the SDK with breadth.**
Add 2–3 connectors of *different shapes* to validate the abstraction: a second SQL engine (MySQL/SQL
Server), an HTTP/REST source (exercises `IHttpClientFactory` injection), and a file/columnar source.
If all three are easy, the SDK is doing its job. Move discovery to plugin loading (§3.9 steps 6–8).

**Phase 3 — Core product features.**
Query parameters (§5.3), result caching + export, then Dashboards (§5.1) as the flagship.

**Phase 4 — Enterprise polish.**
Alerts, audit log, API tokens, scoped share links, retention jobs, SSO/OIDC if not already covered.

---

## 8. One-paragraph "north star"

The thing that will make The Grid genuinely better than Redash is the combination it's already
reaching for: **a real per-org, permissioned authorization model** and **a connector SDK clean enough
that adding a data source is a weekend, not a fork.** Everything in Phases 0–2 is in service of making
those two pillars load-bearing before the feature surface grows on top of them. Get the secrets
encrypted, get the SDK contract right while it's still cheap to change, and the rest is
straightforward product build-out.
