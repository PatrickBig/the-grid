## Why

Phase 2 of the roadmap (`docs/roadmap/ChangeSpecs.md`) calls for SQL Server as the connector SDK's
second RDBMS, specifically to prove the SDK against Azure AD authentication modes that a single-cloud,
single-topology assumption (e.g. AKS-only Workload Identity) would under-serve. The Grid's product goal
is to be enterprise-generic and eventually sellable, not tailored to one company's deployment — SQL
Server / Azure SQL is a common enterprise data source, and its auth options (SQL auth, Azure AD Service
Principal, Azure AD Default Credential Chain) are the first real motivating case for a connector needing
more than a username/password pair.

## What Changes

- Add a `SqlServerConnector` to `TheGrid.Connectors` implementing `ConnectorBase`, `ISchemaDiscovery`,
  `IConnectionTest`, and `IWriteAccessProbe`, using `Microsoft.Data.SqlClient` (the modern, actively
  maintained driver — not the legacy `System.Data.SqlClient`).
- `Query.Command` is plain T-SQL text (no bespoke JSON contract, unlike the MongoDB connector) — SQL
  Server is structurally identical to Postgres from the SDK's point of view. `queryParameters` binds via
  `SqlParameter`/`@paramName`, mirroring `PostgreSqlConnector`'s `AddWithValue` pattern.
- Editor syntax highlighting uses the existing generic `EditorLanguage.Sql` constant — no new
  `Mssql`/`TSql` constant is added (Monaco's `basic-languages` list has no distinct T-SQL grammar).
- Schema discovery (`ISchemaDiscovery.GetSchemaAsync`) queries ANSI-standard
  `INFORMATION_SCHEMA.COLUMNS`/`INFORMATION_SCHEMA.TABLES`, mirroring `PostgreSqlConnector`'s approach —
  not SQL Server's richer `sys.tables`/`sys.columns` catalog views.
- Connection parameters reuse `CommonConnectionParameters` (`ConnectionString`, `DatabaseName`,
  `Username`, `Password`, `PortNumber`) for the SQL-authentication baseline, plus a new
  `AuthenticationMode` parameter selecting between three modes:
  - **SQL Authentication** (baseline) — `Username`/`Password`, exactly like Postgres.
  - **Azure AD Service Principal** — new `TenantId`/`ClientId`/`ClientSecret` parameters (`ClientSecret`
    marked `IsSecret`), building a `ClientSecretCredential` (`Azure.Identity`) and setting
    `SqlConnection.AccessTokenCallback` to fetch a token for scope `https://database.windows.net/.default`.
    This is the primary/recommended Azure AD path — a fully portable, connection-scoped credential that
    works identically across EKS, on-prem K8s, AKS, and local dev.
  - **Azure AD Default Credential Chain** — `DefaultAzureCredential()` (`Azure.Identity`), same
    `AccessTokenCallback` mechanism, zero secrets stored in Grid. Offered as a secondary, zero-secret
    alternative for deployments whose ops team has wired the right federation/managed-identity plumbing.
- `TenantId`/`ClientId`/`ClientSecret` are declared as NOT required at the `[ConnectorParameter]`
  attribute level (existing SDK has no conditional-required mechanism); the connector validates the trio
  itself in code when `AuthenticationMode == ServicePrincipal`, throwing `ConnectorParameterException`
  directly rather than relying on `ConnectorBase`'s blanket required-parameter check. The
  connection-creation UI will show all parameters flatly regardless of selected mode — a known, accepted
  rough edge (tracked as the `VisibleWhen`/`DependsOn` deferred item in `docs/roadmap/ChangeSpecs.md`
  Phase 2+ and `docs/Roadmap.md` §3.2).
- Adds `Microsoft.Data.SqlClient` and `Azure.Identity` package references to `TheGrid.Connectors.csproj`.
- Documentation: `docs/connectors/SQL Server.md` (or the connector's exact display name), matching the
  convention established by `openspec/changes/archive/2026-07-27-add-connector-user-documentation` —
  queued as an unchecked task for a later `/opsx:apply` pass, not written by this proposal.
- **Explicitly out of scope**: on-prem Windows/Kerberos authentication (shared AD service-account
  identity per Connection via a stored keytab + init/sidecar `kinit` pattern in K8s) — deliberately
  tabled as a separate, larger infrastructure thread.

## Capabilities

### New Capabilities
- `sqlserver-connector`: SQL Server connector's plain-T-SQL `Query.Command` contract (parameter binding
  via `SqlParameter`), `INFORMATION_SCHEMA`-based schema discovery, `HAS_PERMS_BY_NAME`-based write-access
  probing, and the three-mode `AuthenticationMode` connector parameter (SQL Authentication, Azure AD
  Service Principal via `ClientSecretCredential`, Azure AD Default Credential Chain via
  `DefaultAzureCredential`) using `SqlConnection.AccessTokenCallback`.

### Modified Capabilities
(none — this connector conforms to existing capabilities such as `connector-streaming`,
`connector-capability-results`, `connector-parameter-identity`, `connector-instantiation`, and
`connection-secret-management` without changing their requirements)

## Impact

- **New code**: `source/TheGrid.Connectors/SqlServerConnector.cs`, plus `Microsoft.Data.SqlClient` and
  `Azure.Identity` package references added to `TheGrid.Connectors.csproj`.
- **New docs** (queued as a task, not written here): `docs/connectors/<Display Name>.md`.
- **No changes to `TheGrid.Connectors.Abstractions`, `ConnectorBase`, or any other SDK contract** — the
  connector fits entirely within the existing `[ConnectorParameter]`/`ConnectorBase` shape; the one gap
  it surfaces (conditional parameter requiredness) is handled by connector-local validation, not an SDK
  change, and is cross-referenced as a still-deferred `VisibleWhen`/`DependsOn` item for a future change.
- **Explicitly out of scope** (deferred to future, separately-proposed changes):
  - On-prem Windows/Kerberos authentication for SQL Server.
  - `VisibleWhen`/`DependsOn` conditional connector-parameter metadata (this change is its first real
    motivating consumer, not its implementer).
