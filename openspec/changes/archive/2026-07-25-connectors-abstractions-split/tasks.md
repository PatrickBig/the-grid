## 1. Create the new project

- [x] 1.1 Create `source/TheGrid.Connectors.Abstractions/TheGrid.Connectors.Abstractions.csproj`
      (net8.0, `Nullable`/`ImplicitUsings` enabled, matching `TheGrid.Connectors.csproj`'s style).
      Package reference: `Mapster` only. Project reference: `TheGrid.Shared`.
- [x] 1.2 Add the new project to `TheGrid.sln` (`dotnet sln TheGrid.sln add
      TheGrid.Connectors.Abstractions/TheGrid.Connectors.Abstractions.csproj` from `source/`).

## 2. Move files (namespaces unchanged — see design.md)

- [x] 2.1 Move `IConnector.cs`, `ConnectorBase.cs`, `ConnectorParameterException.cs` from
      `TheGrid.Connectors/` to `TheGrid.Connectors.Abstractions/`, keeping their existing namespace
      (`TheGrid.Connectors`).
- [x] 2.2 Move `Attributes/ConnectorAttribute.cs`, `Attributes/ConnectorParameterAttribute.cs` to
      `TheGrid.Connectors.Abstractions/Attributes/`, keeping namespace `TheGrid.Connectors.Attributes`.
- [x] 2.3 Move `ISchemaDiscovery.cs`, `IConnectionTest.cs`, `IPermissionTest.cs` to
      `TheGrid.Connectors.Abstractions/`, keeping namespace `TheGrid.Connectors`.
- [x] 2.4 Move `Models/DatabaseObject.cs`, `Models/DatabaseObjectColumn.cs`, `Models/DatabaseSchema.cs` to
      `TheGrid.Connectors.Abstractions/Models/`, keeping namespace `TheGrid.Connectors.Models`.
- [x] 2.5 Move `CommonConnectionParameters.cs` to `TheGrid.Connectors.Abstractions/`, keeping namespace
      `TheGrid.Connectors`.
- [x] 2.6 Move `Extensions/TypeExtensions.cs` and `Extensions/ConnectorExtensions.cs` to
      `TheGrid.Connectors.Abstractions/Extensions/`, keeping namespace `TheGrid.Connectors.Extensions`.
- [x] 2.7 Move (or split, if `TheGrid.Connectors`' remaining `GlobalUsings.cs` still needs its own copy)
      the global usings for `TheGrid.Connectors.Attributes`/`TheGrid.Connectors.Models`/
      `TheGrid.Shared.Models` to `TheGrid.Connectors.Abstractions/GlobalUsings.cs`. Check whether
      `TheGrid.Connectors` (remaining: `PostgreSqlConnector.cs`, `TestConnector.cs`) still needs its own
      `GlobalUsings.cs` for the same usings (it does — those files use the same namespaces) and keep/add
      one there too.

## 3. Update TheGrid.Connectors (remaining, concrete project)

- [x] 3.1 Remove the `Dapper` package reference from `TheGrid.Connectors.csproj` (confirmed unused
      anywhere in the project — dead dependency).
- [x] 3.2 Remove the `Mapster` package reference from `TheGrid.Connectors.csproj` (only used by
      `ConnectorExtensions.cs`, which moved out in task 2.6).
- [x] 3.3 Add a `ProjectReference` to `TheGrid.Connectors.Abstractions.csproj`.
- [x] 3.4 Confirm `PostgreSqlConnector.cs` and `TestConnector.cs` still compile unchanged (they shouldn't
      need any code changes — only their project's dependency graph changed).

## 4. Fix the three assembly-anchor call sites (the critical, easy-to-miss part of this change)

- [x] 4.1 `TheGrid.Services/ConnectorDiscoveryService.cs`'s `GetConnectorTypes()`: change
      `Assembly.GetAssembly(typeof(IConnector))` to `Assembly.GetAssembly(typeof(PostgreSqlConnector))`.
      Add a one-line comment explaining why (anchors on a type guaranteed to be in the concrete
      connectors assembly — see design.md's Decisions for the full rationale and why this is scoped as
      temporary, superseded when `P1-6` adds multi-assembly discovery).
- [x] 4.2 `TheGrid.Services/QueryExecutor.cs`'s `GetConnector()`: same fix,
      `Assembly.GetAssembly(typeof(PostgreSqlConnector))`.
- [x] 4.3 `TheGrid.Server/Controllers/ConnectionsController.cs`'s `ResolveConnectorType()`: same fix.
      This call site was found by grepping the whole solution during design, not listed in
      `docs/roadmap/ChangeSpecs.md`'s original P1-2 file list — don't skip it.
- [x] 4.4 Grep the entire solution (`grep -rn "GetAssembly(typeof(IConnector))"` or equivalent) after
      the above three fixes to confirm there is no fourth call site this design missed.

## 5. Wire up project references for consumers

- [x] 5.1 Add explicit `ProjectReference` to `TheGrid.Connectors.Abstractions.csproj` in
      `TheGrid.Services/TheGrid.Services.csproj` (it already references concrete `TheGrid.Connectors`,
      which will transitively bring Abstractions, but add it explicitly since `ConnectorDiscoveryService.cs`/
      `QueryExecutor.cs` reference Abstractions types — `IConnector`, `ISchemaDiscovery`, `IConnectionTest`,
      `ConnectorAttribute`, `ConnectorParameterAttribute` — directly).
- [x] 5.2 Add explicit `ProjectReference` to `TheGrid.Connectors.Abstractions.csproj` in
      `TheGrid.Server/TheGrid.Server.csproj` (`ConnectionsController.cs` references
      `TheGrid.Connectors`/`TheGrid.Connectors.Extensions` types directly).
- [x] 5.3 Add `ProjectReference` to `TheGrid.Connectors.Abstractions.csproj` in
      `tests/TheGrid.Tests.Connectors/TheGrid.Tests.Connectors.csproj` (keeps existing tests for moved
      types compiling from the same test project — see design.md for why no new test project is created).

## 6. Tests

- [x] 6.1 Confirm existing tests for moved types (`ConnectorParameterAttributeTests`,
      `TypeExtensionsTests`, `ConnectorExtensionsTests`, etc. in `TheGrid.Tests.Connectors`) still compile
      and pass unchanged — they shouldn't need edits since namespaces didn't change, only which project
      physically contains the type.
- [x] 6.2 Add a test (in `TheGrid.Tests.Services`) asserting `ConnectorDiscoveryService.RefreshConnectorsAsync()`
      still discovers both `PostgreSqlConnector` and `TestConnector` post-split — this is the primary
      regression guard for the anchor-type risk.
- [x] 6.3 Add/confirm a test (in `TheGrid.Tests.Services`) that `QueryExecutor` can resolve and
      instantiate `PostgreSqlConnector` by its `ConnectorId` post-split.
- [x] 6.4 Add/confirm a test (in `TheGrid.Tests.Server`) that `ConnectionsController`'s connector-secret-key
      resolution still works post-split (doesn't throw "no connector found" for a valid `ConnectorId`).

## 7. Verification

- [x] 7.1 `dotnet build TheGrid.sln` (or each project individually if the solution build hits the known
      pre-existing `docker-compose.dcproj`/`NU1105` issue — confirm that's still the only failure and
      it's unrelated to this change) — no errors, and specifically confirm `TheGrid.Connectors.Abstractions`
      itself builds standalone with no `Npgsql` in its dependency graph.
- [x] 7.2 Run the full test suite project-by-project — all green.
- [x] 7.3 Manual end-to-end check via the running app: bring the stack up with
      `docker-compose -f source/docker-compose.yml -f source/docker-compose.override.yml up` from
      `source/`, hit `GET /api/v1/Connectors` and confirm both connectors are listed (proves discovery
      works end-to-end, not just in unit tests), create a connection and run a query against it (proves
      execution still resolves the connector correctly), then `... down` immediately after. Do not use
      `dotnet run` directly or leave the stack running.
