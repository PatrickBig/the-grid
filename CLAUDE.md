# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

The Grid is a self-hosted data query/dashboarding platform (think a lightweight Redash/Metabase). Users connect to data sources ("connections") via pluggable "connectors", write queries against them, execute those queries (sync or via background jobs), and render the results as visualizations (tables, etc.) on dashboards. Access is scoped per-organization with group-based permissions.

All source lives under `source/`. The solution file is `source/TheGrid.sln`.

## Commands

Run from the `source/` directory unless noted.

```
dotnet restore TheGrid.sln
dotnet build TheGrid.sln
dotnet test TheGrid.sln
```

Run a single test project:
```
dotnet test tests/TheGrid.Tests.Services/TheGrid.Tests.Services.csproj
```

Run a single test (xunit fully-qualified name):
```
dotnet test tests/TheGrid.Tests.Services/TheGrid.Tests.Services.csproj --filter "FullyQualifiedName~QueryManagerTests.MethodName"
```

Add an EF Core migration (must specify provider + connection string; providers are `postgresql` and `sqlite` per `docs/AddingMigrations.md`):
```
dotnet ef migrations add {MigrationName} --project TheGrid.Postgres\TheGrid.Postgres.csproj --startup-project TheGrid.Server\TheGrid.Server.csproj -- {ProviderName} {ConnectionString}
```

**Schema is still unstable (pre-v1): do not add incremental migrations.** On a schema change, delete
the existing migration(s) for both providers and regenerate a single fresh `Initial` migration, then
wipe the local dev DB rather than trying to apply an incremental one on top of stale data —
`scripts/dev.sh remigrate && scripts/dev.sh reset-db` does both steps. Switch to normal incremental
migrations once the schema stabilizes (post-v1 / once there's real data worth preserving).

## Dev workflow helper

`scripts/dev.sh <command>` wraps the common local dev tasks (docker-compose stack, `/setup`, DB reset,
test-org seeding, migration regeneration) behind a small fixed set of subcommands, so the whole script
can be allowlisted once instead of prompting per docker-compose/dotnet invocation. Run
`scripts/dev.sh help` for the full list; the commands are: `up`, `down`, `status`, `setup`, `reset-db`,
`seed-test-org [name] [slug]`, `remigrate`. Prefer this over raw `docker-compose`/`dotnet ef` calls for
anything it already covers.

Local dev stack (Postgres, Redis, Adminer, server) via Docker Compose — `scripts/dev.sh up` / `down`,
or directly:
```
docker-compose -f source/docker-compose.yml -f source/docker-compose.override.yml up
```
The `thegrid.setup` service runs the server with the `/setup` argument to apply migrations and seed the default admin user/group (`DEFAULT_ADMIN_PASSWORD` env var, default `TheGrid123!` in `docker-compose.yml`). `thegrid.setup`/`thegrid.server` both wait on Postgres's healthcheck (`depends_on: condition: service_healthy`) before starting — needed because a fresh/wiped volume takes longer to initialize than a warm one. `RunMode` (`Server`, `Agent`, `Mixed`) in `SystemOptions` controls which services a given process instance runs — see `TheGrid.Server/Program.cs` and `StartupHelpers.cs`.

CI (`.github/workflows/sonar-scan.yml`) builds and tests the whole solution and uploads coverage/SonarCloud analysis on pushes to `main` and PRs touching `source/**`.

## Architecture

Solution structure (each is its own project under `source/`):

- **TheGrid.Server** — ASP.NET Core host. Controllers (`Controllers/`), ASP.NET Identity + custom authorization (`Security/`), and the `/setup` bootstrap hosted service (`Setup/SetupHostedService.cs`). Hosts both the API and the Blazor WASM client (`UseBlazorFrameworkFiles`, `MapFallbackToFile("index.html")`), plus a SignalR hub (`QueryDesignerHub`) and Hangfire dashboard.
- **TheGrid.Client** — Blazor WebAssembly frontend (pages, shared components, SignalR hub clients).
- **TheGrid.Services** — Core business logic: managers (`GroupManager`, `OrganizationManager`, `QueryManager`, `QueryExecutor`, `QueryRefreshManager`, `TableVisualizationManager`, `VisualizationManagerFactory`), each defined against an interface (`I*Manager`) for DI/testability. Also owns cross-cutting authorization handlers (`Authorization/`) and SignalR hubs (`Hubs/`).
- **TheGrid.Models** — EF Core entity classes (`GridUser`, `Organization`, `Group`, `GroupPermission`, `Connection`, `Query`, `QueryExecution`, etc.) and configuration POCOs bound from `appsettings.json` (`Configuration/SystemOptions.cs`, `EmailOptions.cs`).
- **TheGrid.Data** — `TheGridDbContext` (EF Core) and `TheGridContextFactory` (design-time factory used by `dotnet ef`, selects Npgsql vs Sqlite based on `SystemOptions.DatabaseProvider`).
- **TheGrid.Postgres** / **TheGrid.Sqlite** — Provider-specific EF Core migrations assemblies only. Each contains a `Migrations/` folder and an `Assembly.cs` marker; no other logic belongs here.
- **TheGrid.Connectors** — Pluggable query runner implementations (`PostgreSqlConnector`, `TestConnector`), discovered via `ConnectorDiscoveryService` (in Services) by reflecting over classes inheriting `ConnectorBase` tagged with `[QueryRunner]`. See `docs/Creating-Connectors.md` for the full guide to adding a new connector (base class, `RunQueryAsync`, `QueryRunnerParameterAttribute`s for connection parameters, optional `ISchemaDiscovery`/`IConnectionTest`/`IPermissionTest`).
- **TheGrid.Shared** — Code shared between server and Blazor client: DTOs/request-response models (`Models/`), permission/claims constants (`Constants/ApplicationPermission.cs`, `GridClaimTypes.cs`, `BuiltInGroups.cs`), and `ClaimsPrincipalExtensions` (e.g. `IsSystemAdministrator()`, `IsMemberOfOrganization()`).

### Authorization model

- `ApplicationPermission` (`TheGrid.Shared/Constants/ApplicationPermission.cs`) enumerates every fine-grained permission (e.g. `CreateConnection`, `ApproveQueries`, `ViewQuerySource`). Permissions are attached to a `Group` via `GroupPermission`, and users get permissions by `UserGroup` membership.
- Every `Group` belongs to an `Organization` (`OrganizationId`) except built-in system groups (e.g. `SystemAdministrator`), which have `OrganizationId == null` and are managed through separate `GroupManager` methods (`CreateSystemAdministratorGroupAsync`, not `CreateGroupAsync`) — don't conflate the two paths.
- Multi-tenancy/org-scoping is enforced two ways: the `OrganizationPolicy` authorization policy (`OrganizationHandler` + `OrganizationRequirement`) checks the `ApplicationHeaders.OrganizationId` request header against the caller's `GridClaimTypes.Organization` claim (or `IsSystemAdministrator()`); resource-level checks (e.g. `ConnectionAuthorizationHandler`) use ASP.NET Core's resource-based `AuthorizationHandler<OperationAuthorizationRequirement, T>` pattern against `GridOperations.Read`/`Create`.
- See `docs/Permissions.md` for the permission list and the concept of "permission models" chosen at organization setup (defaults for new groups).

### Query execution flow

A `Query` targets a `Connection` (which references a connector by name). `QueryExecutor` runs a query through the resolved `IConnector`, producing a `QueryExecution`/`QueryResultRow` set; `QueryRefreshManager` handles scheduled re-execution (via Hangfire). Visualization rendering of results is handled by `IVisualizationManager` implementations created through `VisualizationManagerFactory` (currently `TableVisualizationManager`); `VisualizationInformation` describes what visualization types/options are available for a result set.

## Code style

- StyleCop.Analyzers is enforced solution-wide (`Directory.build.props` + `stylecop.json`/`.editorconfig`); company name in file headers is `BiglerNet`. Every new `.cs` file needs the standard header comment:
  ```csharp
  // <copyright file="{FileName}.cs" company="BiglerNet">
  // Copyright (c) BiglerNet. All rights reserved.
  // </copyright>
  ```
- XML doc comments are required on public members (`GenerateDocumentationFile` is on) — constructors, methods, and properties consistently have `<summary>`/`<param>`/`<returns>` docs; match this when adding public APIs.
- Indentation uses spaces, not tabs (`stylecop.json`).
- Services take dependencies via primary constructors (e.g. `public class GroupManager(TheGridDbContext db, ILogger<GroupManager> logger) : IGroupManager`) and assign to private readonly fields; follow this pattern for new manager/service classes rather than traditional constructor bodies.
- Every service/manager is defined behind an interface in `TheGrid.Services` (`I*Manager`, `I*Executor`) — implement new business logic the same way for DI and testability.

## Testing

- Tests use xunit + NSubstitute (`TheGrid.Tests.*` projects mirror the corresponding source project name) and `Microsoft.EntityFrameworkCore.InMemory` or the Sqlite provider (`TheGrid.TestHelpers/SqliteProvider.cs`) for DB-backed tests rather than a live Postgres instance.
- `tests/fixtures/*.sql` holds seed data used by connector/integration-style tests.
- `CodeCoverage.runsettings` (referenced from `Directory.build.props`) configures coverage collection for `dotnet test`.
