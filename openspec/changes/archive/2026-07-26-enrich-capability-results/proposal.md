## Why

`IConnectionTest.TestConnectionAsync` returns a bare `bool` — when a connection test fails, a caller
gets no reason why. Worse, `PostgreSqlConnector`'s current implementation doesn't even honor that
contract correctly: it opens the connection, constructs (but never executes) a `"select 1"` command,
and unconditionally returns `true` — a connection failure isn't caught and translated to `false`, it
propagates as an uncaught exception instead (confirmed by the existing test
`TestConnection_Fails_Test`, which asserts `ThrowsAnyAsync<Exception>`, not `Assert.False`).
Separately, `IPermissionTest`'s name reads like an app-permissions check rather than what it actually
is — a probe for whether a connection's credentials have write access. `docs/roadmap/ChangeSpecs.md`'s
P1-5 covers both.

## What Changes

- **BREAKING**: `IConnectionTest.TestConnectionAsync` changes from `Task<bool>` to
  `Task<ConnectionTestResult>`, where `ConnectionTestResult` is a new record
  `(bool Success, string? Message, TimeSpan Elapsed)` in `TheGrid.Connectors.Abstractions`.
- **BREAKING rename**: `IPermissionTest` → `IWriteAccessProbe`; its method
  `HasWritePermissionAsync` → `HasWriteAccessAsync` (renamed together for a name that reads
  consistently — "does this connection have write access," not "does it have permission").
- `PostgreSqlConnector.TestConnectionAsync` rewritten to actually execute the test query (not just
  construct it), measure elapsed time via `Stopwatch`, and catch connection/execution failures,
  returning `ConnectionTestResult(false, ex.Message, elapsed)` instead of letting the exception
  propagate. This is a deliberate behavior fix, not just a signature change — the existing
  `TestConnection_Fails_Test` test's expectation flips from "throws" to "returns `Success: false`."
- `ConnectorDiscoveryService` gains a `SupportsWriteAccessProbe` discovery flag on `Connector`
  (computed via `ImplementsInterface<IWriteAccessProbe>()`), matching the existing
  `SupportsConnectionTest`/`SupportsSchemaDiscovery` pattern — today `IPermissionTest` has **no**
  discovery-time flag at all, an inconsistency with the other two capability interfaces. Requires an EF
  migration (both providers) for the new `Connector.SupportsWriteAccessProbe` column.

## Explicitly out of scope (scoped down from the roadmap's literal file list)

`docs/roadmap/ChangeSpecs.md`'s P1-5 lists "connection controller + client display" and "surface the
result as a UI warning badge" as part of this item. Grepping the whole solution confirms **no endpoint
or UI anywhere currently invokes `TestConnectionAsync` or `HasWritePermissionAsync` against a live
connection** — `SupportsConnectionTest`/`SupportsSchemaDiscovery` are persisted flags with no consumer
UI either. Building an actual "test this connection" API endpoint and a client-side warning badge is
real, net-new feature work (fits `docs/Roadmap.md` §6's UI/UX pass and §5.5's "Supporting features"),
not a Phase 1 SDK-contract change — there's no existing call site to enrich, only a contract to define
correctly before anything is built against it. This change makes the *contract* correct (result type,
naming, discovery-flag parity); wiring an actual test-connection feature into the API/UI is left for a
future, separately-scoped feature change.

## Capabilities

### New Capabilities
- `connector-capability-results`: defines the `ConnectionTestResult` contract for `IConnectionTest`, the
  `IWriteAccessProbe` capability interface, and discovery-flag parity across all three capability
  interfaces (`ISchemaDiscovery`, `IConnectionTest`, `IWriteAccessProbe`).

### Modified Capabilities
(none — no existing spec covers this behavior yet)

## Impact

- `TheGrid.Connectors.Abstractions/IConnectionTest.cs` — return type change.
- `TheGrid.Connectors.Abstractions/IPermissionTest.cs` → renamed `IWriteAccessProbe.cs`, method renamed.
- `TheGrid.Connectors.Abstractions/ConnectionTestResult.cs` (new).
- `TheGrid.Connectors/PostgreSqlConnector.cs` — both method implementations rewritten.
- `TheGrid.Services/ConnectorDiscoveryService.cs` — new discovery flag.
- `TheGrid.Shared/Models/Connector.cs` — new `SupportsWriteAccessProbe` bool property.
- EF migrations in `TheGrid.Postgres` and `TheGrid.Sqlite` for the new column.
- `tests/TheGrid.Tests.Connectors/PostgreSqlConnectorTests.cs` — updated assertions (including the
  intentional throws→returns-false behavior flip), `tests/TheGrid.Tests.Services/ConnectorDiscoveryServiceTests.cs`.
- No controller or client changes (see "Explicitly out of scope").
