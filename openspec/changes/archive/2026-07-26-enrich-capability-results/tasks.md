## 1. New result type and interface changes

- [x] 1.1 Add `ConnectionTestResult.cs` to `TheGrid.Connectors.Abstractions` (namespace `TheGrid.Connectors`):
      a sealed record `ConnectionTestResult(bool Success, string? Message, TimeSpan Elapsed)`.
- [x] 1.2 Change `IConnectionTest.TestConnectionAsync` to return `Task<ConnectionTestResult>` instead of
      `Task<bool>`.
- [x] 1.3 Rename `IPermissionTest.cs` → `IWriteAccessProbe.cs` (rename the file too), interface
      `IPermissionTest` → `IWriteAccessProbe`, method `HasWritePermissionAsync` → `HasWriteAccessAsync`
      (keep the return type `Task<bool>` — only `IConnectionTest`'s result is being enriched, not this
      one).

## 2. PostgreSqlConnector

- [x] 2.1 Rewrite `TestConnectionAsync`: measure elapsed time with a `Stopwatch`, actually execute the
      test query (e.g. `ExecuteScalarAsync` on the existing `"select 1"` command — today it's
      constructed but never run), catch `Exception` broadly around the open+execute (see design.md for
      why broad, not Npgsql-specific), returning `ConnectionTestResult(false, ex.Message, stopwatch.Elapsed)`
      on failure and `ConnectionTestResult(true, null, stopwatch.Elapsed)` on success.
- [x] 2.2 Rename `HasWritePermissionAsync` → `HasWriteAccessAsync`, implementing `IWriteAccessProbe`
      instead of `IPermissionTest`. Update the class declaration
      (`PostgreSqlConnector : ConnectorBase, ISchemaDiscovery, IConnectionTest, IWriteAccessProbe`).

## 3. Discovery flag parity

- [x] 3.1 Add `SupportsWriteAccessProbe` (bool) to `TheGrid.Shared.Models.Connector`.
- [x] 3.2 In `ConnectorDiscoveryService.DiscoverConnectors()`, add
      `details.SupportsWriteAccessProbe = connectorType.ImplementsInterface<IWriteAccessProbe>();`
      alongside the existing `SupportsConnectionTest`/`SupportsSchemaDiscovery` lines.

## 4. EF migration

- [x] 4.1 Add a new EF Core migration (e.g. `ConnectorWriteAccessProbeSupport`) in `TheGrid.Postgres` for
      the new `Connector.SupportsWriteAccessProbe` column — an ordinary incremental migration, not
      another squash (see design.md's Decisions).
- [x] 4.2 Add the equivalent migration in `TheGrid.Sqlite`.
- [x] 4.3 Verify `TheGridContextFactory` design-time creation still works for both providers (per
      `docs/AddingMigrations.md`), and that `TheGridDbContextModelSnapshot.cs` regenerated correctly for
      both.

## 5. Tests

- [x] 5.1 Update `TestConnection_Test` (`PostgreSqlConnectorTests.cs`) to assert on the new
      `ConnectionTestResult` shape (`result.Success == true`, `result.Elapsed` non-negative) instead of
      `Assert.True(result)`.
- [x] 5.2 Update `TestConnection_Fails_Test` — this is an intentional behavior change, not a mechanical
      fix: it previously asserted `Assert.ThrowsAnyAsync<Exception>`; it should now assert the method
      returns normally with `result.Success == false` and a non-null `result.Message`.
- [x] 5.3 Update any test exercising `HasWritePermissionAsync`/`IPermissionTest` to the renamed
      `HasWriteAccessAsync`/`IWriteAccessProbe` (grep to find all references, don't rely on this being
      exhaustive).
- [x] 5.4 Add/update a `ConnectorDiscoveryServiceTests.cs` test asserting `PostgreSqlConnector`'s
      discovered `Connector` row has `SupportsWriteAccessProbe == true` (it implements the interface),
      mirroring existing coverage for `SupportsConnectionTest`/`SupportsSchemaDiscovery`.

## 6. Verification

- [x] 6.1 `dotnet build TheGrid.sln` (or per-project if the solution build hits the known pre-existing
      `docker-compose.dcproj`/`NU1105` issue — confirm unrelated) — no errors.
- [x] 6.2 Run the full test suite project-by-project — all green.
- [x] 6.3 Manual end-to-end check via Docker Compose ONLY (never `dotnet run` directly):
      `docker-compose -f source/docker-compose.yml -f source/docker-compose.override.yml up` from
      `source/`, confirm the app starts cleanly and applies the new migration without error (check
      `GET /api/v1/Connectors` still lists both connectors with the new field present in the response),
      then `... down` immediately after. Do not leave the stack running.
