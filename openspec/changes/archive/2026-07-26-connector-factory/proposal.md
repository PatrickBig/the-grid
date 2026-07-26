## Why

`QueryExecutor.GetConnector()` constructs every connector via raw `Activator.CreateInstance(connectorType,
connectionProperties)`. This means a connector can never receive an `ILogger`, an `IHttpClientFactory`
(essential for HTTP/REST-based sources), or any other cross-cutting service — every connector is on its
own. `docs/roadmap/ChangeSpecs.md`'s P1-3 calls for an `IConnectorFactory` abstraction that constructs
connectors through a context object instead of raw reflection, so connectors get access to shared
infrastructure and the host gets a seam to test/replace/extend instantiation.

## What Changes

- **New** `ConnectorContext` record in `TheGrid.Connectors.Abstractions`:
  `ConnectorContext(Dictionary<string, string> Parameters, ILoggerFactory LoggerFactory,
  IHttpClientFactory HttpClientFactory)`. Scoped down from the roadmap's illustrative sketch — see
  "Explicitly out of scope" below.
- **BREAKING**: `ConnectorBase`'s constructor changes from `ConnectorBase(Dictionary<string, string>
  connectorParameters)` to `ConnectorBase(ConnectorContext context)`, exposing `ConnectorParameters`
  (from `context.Parameters`, unchanged type/behavior) plus new protected `LoggerFactory` and
  `HttpClientFactory` properties. `PostgreSqlConnector`/`TestConnector` constructors update to match.
- **New** `IConnectorFactory.Create(string connectorId, Dictionary<string, string> parameters) :
  IConnector`, implemented by a new `ConnectorFactory` class in `TheGrid.Services`. The factory itself
  is constructed with `ILoggerFactory`/`IHttpClientFactory` (DI-injected once, at the factory level) and
  builds the `ConnectorContext` internally before resolving the connector type and calling
  `Activator.CreateInstance(type, context)` — this keeps the cross-cutting-resource wiring in one place
  and means `QueryExecutor` only gains a single new dependency (`IConnectorFactory`), not three.
- `QueryExecutor.GetConnector()` rewritten to call `_connectorFactory.Create(connectorId, parameters)`
  instead of doing its own `Assembly.GetAssembly`/`Activator.CreateInstance` — the raw-reflection
  instantiation this roadmap item exists to remove.
- `IConnectorFactory`/`ConnectorFactory` registered in DI (`TheGridContextServices.cs`).
- **Gap found and fixed as part of this change**: `IHttpClientFactory` is currently registered nowhere
  server-side (`AddHttpClient()` is only called in `TheGrid.Client`, the Blazor WASM project) — without
  adding `services.AddHttpClient()` to the server's DI setup, `ConnectorFactory`'s constructor
  dependency would fail to resolve at runtime.
- `ConnectorId == type.FullName` semantics preserved — this change only changes *how* a connector is
  constructed, not its identity.

## Explicitly out of scope (scoped down from the roadmap's illustrative sketch)

The roadmap's `ConnectorContext` sketch also included `ExecutionLimits Limits`. Not included here:
`ExecutionLimits` lives in `TheGrid.Models.Configuration`, and `TheGrid.Models.csproj` references
`Microsoft.EntityFrameworkCore.Design`/`Microsoft.AspNetCore.Identity.EntityFrameworkCore` — pulling
`ExecutionLimits` into `ConnectorContext` would force `TheGrid.Connectors.Abstractions` to reference a
project with EF dependencies, directly violating the "no EF" bar the Abstractions split (`P1-2`) exists
to enforce. More importantly: **no connector today consults execution limits at all** — `MaxRows`/timeout
enforcement happens entirely in `QueryExecutor` externally (stopping enumeration, cancellation token),
not inside `PostgreSqlConnector`/`TestConnector`. Adding a dependency nothing uses, at the cost of a real
architectural constraint violation, is exactly the premature-abstraction this project avoids. Similarly
not included: a "secret resolver" hook (roadmap §3.6 mentions it as a future possibility, not a current
need).

## Capabilities

### New Capabilities
- `connector-instantiation`: connectors are constructed via `IConnectorFactory` with access to shared
  `ILoggerFactory`/`IHttpClientFactory` infrastructure, not via raw reflection.

### Modified Capabilities
(none — no existing capability's requirements change)

## Impact

- `TheGrid.Connectors.Abstractions/ConnectorContext.cs` (new), `ConnectorBase.cs` (ctor change).
- `TheGrid.Connectors.Abstractions.csproj` — new package references for `ILoggerFactory`/
  `IHttpClientFactory` types (lightweight `Microsoft.Extensions.*` abstraction packages — distinct from
  the Npgsql/EF/Dapper-class dependencies the split exists to keep out).
- `TheGrid.Connectors/PostgreSqlConnector.cs`, `TestConnector.cs` — constructor signature update.
- `TheGrid.Services/IConnectorFactory.cs` (new), `ConnectorFactory.cs` (new), `QueryExecutor.cs`
  (`GetConnector()` rewritten).
- `TheGrid.Services/DependencyInjection/TheGridContextServices.cs` — register `IConnectorFactory` +
  `AddHttpClient()`.
- Tests: `PostgreSqlConnectorTests.cs`, `TestConnectorTests.cs`, `ConnectorExtensionsTests.cs` (all
  construct connectors directly with a raw dictionary — ~14 call sites need a `ConnectorContext` now),
  `QueryExecutorTests.cs` (~8 call sites constructing `QueryExecutor` directly — need a mocked
  `IConnectorFactory` instead of relying on real `Activator` resolution).
- No EF migration, no API contract changes, no data changes.
