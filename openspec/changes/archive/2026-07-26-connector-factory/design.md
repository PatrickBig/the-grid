## Context

`QueryExecutor.GetConnector()` (post the `connectors-abstractions-split` change) does:
```csharp
var connectorAssembly = Assembly.GetAssembly(typeof(PostgreSqlConnector));
var connectorType = connectorAssembly?.GetType(query.Connection!.ConnectorId) ?? throw ...;
var connectionProperties = new Dictionary<string, string?>(query.Connection.ConnectionProperties);
foreach (secret in query.Connection.SecretProperties) { connectionProperties[key] = decrypted; }
return Activator.CreateInstance(connectorType, connectionProperties) as IConnector ?? throw ...;
```
`ConnectorBase`'s constructor takes `Dictionary<string, string> connectorParameters` directly. Nothing
provides a connector with logging or HTTP infrastructure — every connector is entirely on its own for
cross-cutting concerns, per `docs/Roadmap.md` §3.6.

Two things confirmed while grounding this change:
1. `IHttpClientFactory` has no server-side DI registration anywhere — `AddHttpClient()` is called only
   in `TheGrid.Client/Program.cs` (the Blazor WASM client). The server (`TheGrid.Server`/
   `TheGrid.Services`) has never needed it before now.
2. `QueryExecutor` is constructed directly (not through DI) in ~8 places across
   `QueryExecutorTests.cs`, and `PostgreSqlConnector`/`TestConnector` are constructed directly in ~14
   places across `PostgreSqlConnectorTests.cs`/`TestConnectorTests.cs`/`ConnectorExtensionsTests.cs`.
   Both numbers matter for the design below — a change that adds three new constructor parameters to
   `QueryExecutor` would touch all 8 test call sites for parameters most of them don't care about.

## Goals / Non-Goals

**Goals:**
- Connectors can receive `ILoggerFactory`/`IHttpClientFactory` through their constructor.
- `QueryExecutor` no longer does raw `Assembly`/`Activator` reflection — that responsibility moves
  behind `IConnectorFactory`.
- `ConnectorId == type.FullName` keeps working unchanged (existing `Connection` rows still resolve).
- Minimize the test-ripple surface where possible without compromising the design.

**Non-Goals:**
- `ExecutionLimits` in `ConnectorContext` — see proposal.md's "Explicitly out of scope."
- A secret-resolver hook — no current need, not built.
- Connector pooling/caching — `docs/Roadmap.md` §3.6 mentions this as a future benefit of having a
  factory seam, not a requirement of this change.

## Decisions

**The factory owns `ILoggerFactory`/`IHttpClientFactory`, not the caller — `ConnectorContext` is built
inside `ConnectorFactory.Create`, not by `QueryExecutor`.**
Considered having `QueryExecutor` build the full `ConnectorContext` itself and pass it to
`IConnectorFactory.Create(string connectorId, ConnectorContext context)` (closer to the roadmap's
literal illustrative signature). Rejected: that would require injecting `ILoggerFactory` and
`IHttpClientFactory` into `QueryExecutor`'s own constructor — two more parameters on top of
`IConnectorFactory` itself, rippling into all ~8 direct `new QueryExecutor(...)` test call sites for
dependencies the tests don't otherwise care about. Instead, `IConnectorFactory.Create(string connectorId,
Dictionary<string, string> parameters)` takes just the parameters; `ConnectorFactory`'s own constructor
takes `ILoggerFactory`/`IHttpClientFactory` (via DI, once) and builds the `ConnectorContext` internally.
`QueryExecutor` gains exactly one new constructor dependency (`IConnectorFactory`, trivially mockable via
NSubstitute), and the "where do cross-cutting connector resources come from" question has exactly one
answer (the factory), which also matches the roadmap's framing of the factory as the seam for future
resource injection (pooling, secret resolution) — that responsibility belongs to the factory, not
scattered into every caller.

**`ConnectorContext.Parameters` stays `Dictionary<string, string>` (non-nullable value), matching
`ConnectorBase`'s existing `ConnectorParameters` contract exactly.**
`QueryExecutor` currently builds a `Dictionary<string, string?>` (nullable values, matching
`Connection.ConnectionProperties`/`SecretProperties`'s storage type) — converting to the non-nullable
shape needs an explicit `.ToDictionary(kv => kv.Key, kv => kv.Value ?? string.Empty)` before calling
`_connectorFactory.Create(...)`. This is a one-line conversion at the one call site that needs it, versus
changing `ConnectorBase`'s long-established `Dictionary<string, string>` contract (which
`ValidateParameters`, `PostgreSqlConnector.GetConnection`, etc. all already assume) to be nullable
everywhere for no benefit.

**`ConnectorContext` lives in `TheGrid.Connectors.Abstractions`, which gains lightweight
`Microsoft.Extensions.Logging.Abstractions`/`Microsoft.Extensions.Http` package references.**
Both are framework abstraction packages (interface-only, no driver/ORM code) — consistent with the
Abstractions project's "no Npgsql, no EF, no services" bar from `docs/roadmap/ChangeSpecs.md` (P1-2),
which is about not forcing connector authors to depend on this app's own heavyweight infrastructure, not
about avoiding thin framework interfaces they'd need anyway to receive a logger/HTTP client.

**`IConnectorFactory`/`ConnectorFactory` live in `TheGrid.Services`, following the existing
`I*Manager`/`I*Executor` convention (interface + implementation, DI-registered) rather than in
Abstractions.**
This is host-side construction logic (reflection + `Activator.CreateInstance`), not something a
connector author implements — it belongs with `IQueryExecutor`, `IGroupManager`, etc.

**Test ripple: add a small internal helper in `TheGrid.Tests.Connectors` rather than pulling in
`TheGrid.TestHelpers`.**
The ~14 call sites needing a `ConnectorContext` are all within `TheGrid.Tests.Connectors` itself
(`PostgreSqlConnectorTests.cs`, `TestConnectorTests.cs`, `ConnectorExtensionsTests.cs`). A small
private/internal helper (e.g. a method building a `ConnectorContext` from just a parameters dictionary,
using `NullLoggerFactory.Instance` and a minimal fake/no-op `IHttpClientFactory`) local to that test
project avoids repeating the boilerplate without adding a new cross-project test dependency for
something this contained.

## Risks / Trade-offs

- **[Risk]** Forgetting to add `services.AddHttpClient()` server-side means `ConnectorFactory`'s
  constructor dependency fails to resolve at runtime (DI validation error), not at compile time.
  **[Mitigation]** tasks.md requires this explicitly, plus the Docker-based manual end-to-end check
  (creating a connection and running a query) is the concrete proof DI resolves correctly — a unit test
  with a mocked `IHttpClientFactory` wouldn't catch a missing `AddHttpClient()` registration.
- **[Risk]** ~14 + ~8 test call sites is a real ripple; missing one is a compile error (good — caught
  immediately), but the sheer count makes it easy to introduce inconsistent fakes/mocks across them.
  **[Mitigation]** The shared internal test helper (see Decisions) means there's one place defining what
  a "test `ConnectorContext`" looks like, not 14 ad-hoc ones.

## Migration Plan

No EF migration, no data changes. Purely a constructor-signature and DI-wiring change. Rollback is a
straight commit revert if something's found broken.

## Open Questions

None — the two things worth deciding (who builds the context, where `ConnectorContext` lives) are
resolved above.
