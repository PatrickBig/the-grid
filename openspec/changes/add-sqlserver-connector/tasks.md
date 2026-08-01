## 1. Setup

- [ ] 1.1 Add `Microsoft.Data.SqlClient` and `Azure.Identity` package references to
      `TheGrid.Connectors.csproj`.
- [ ] 1.2 Scaffold `source/TheGrid.Connectors/SqlServerConnector.cs`:
      `[Connector("SQL Server", EditorLanguage = EditorLanguage.Sql, IconFileName = "sqlserver.png")]`
      class inheriting `ConnectorBase(ConnectorContext context)`, implementing `ISchemaDiscovery`,
      `IConnectionTest`, `IWriteAccessProbe`.
- [ ] 1.3 Declare `[ConnectorParameter]`s: `ConnectionString`, `DatabaseName`, `Username`, `Password`,
      `PortNumber` (using the same `CommonConnectionParameters` keys `PostgreSqlConnector` uses,
      `Required = true`, same shape as Postgres's baseline), `AuthenticationMode` (required, e.g. a
      single-select/text parameter with values `SqlAuthentication` / `ServicePrincipal` /
      `DefaultCredential`), and `TenantId`/`ClientId`/`ClientSecret` (`Required = false`, `ClientSecret`
      with `IsSecret = true`).

## 2. Connection establishment

- [ ] 2.1 Implement a private connection-builder helper (mirroring
      `PostgreSqlConnector.GetConnection`'s shape) that constructs a `SqlConnectionStringBuilder` from
      `ConnectionString`, overriding `Database`/`Port` from `DatabaseName`/`PortNumber` when present.
- [ ] 2.2 Implement SQL Authentication mode: set `SqlConnectionStringBuilder.UserID`/`Password` from
      `Username`/`Password` when `AuthenticationMode == SqlAuthentication`.
- [ ] 2.3 Implement Azure AD Service Principal mode: when `AuthenticationMode == ServicePrincipal`,
      validate `TenantId`/`ClientId`/`ClientSecret` are all present and non-empty — throw
      `ConnectorParameterException` naming the missing parameter(s) if not — then construct a
      `ClientSecretCredential(tenantId, clientId, clientSecret)` and set
      `SqlConnection.AccessTokenCallback` to acquire a token for scope
      `https://database.windows.net/.default`, returning a `SqlAuthenticationToken` per
      `Microsoft.Data.SqlClient`'s documented `AccessTokenCallback` contract (Context7 `/dotnet/sqlclient`).
- [ ] 2.4 Implement Azure AD Default Credential Chain mode: when `AuthenticationMode ==
      DefaultCredential`, construct a `DefaultAzureCredential()` and set
      `SqlConnection.AccessTokenCallback` the same way as 2.3, with no `TenantId`/`ClientId`/
      `ClientSecret` validation required.
- [ ] 2.5 Do NOT use connection-string `Authentication=` keywords or the
      `Microsoft.Data.SqlClient.Extensions.Azure` package for either Azure AD mode — use
      `AccessTokenCallback` with an explicitly constructed `Azure.Identity` credential per design.md
      Decision 5.

## 3. GetDataAsync (streaming query execution)

- [ ] 3.1 Implement `SqlServerConnector.GetDataAsync` as `async IAsyncEnumerable<ConnectorRow>` per the
      `IConnector`/`ConnectorBase` streaming contract (matching signature incl.
      `[EnumeratorCancellation] CancellationToken`), building a `SqlCommand` from the plain T-SQL
      `query` text.
- [ ] 3.2 Bind `queryParameters` (when non-null/non-empty) via `command.Parameters.AddWithValue(key,
      value ?? DBNull.Value)`, mirroring `PostgreSqlConnector.GetDataAsync`'s parameter-binding loop.
- [ ] 3.3 Build `ConnectorRow.Columns` once from the first read row (field name + mapped
      `QueryResultColumnType` via the existing type-mapping extension), reused across the stream,
      consistent with `PostgreSqlConnector`'s pattern.
- [ ] 3.4 Propagate `cancellationToken` through `OpenAsync`/`ExecuteReaderAsync`/`ReadAsync`.

## 4. Schema discovery

- [ ] 4.1 Implement `ISchemaDiscovery.GetSchemaAsync()`: query `INFORMATION_SCHEMA.COLUMNS` joined to
      `INFORMATION_SCHEMA.TABLES` (per design.md Decision 3 — not `sys.tables`/`sys.columns`),
      producing one `DatabaseObject` per table/view.
- [ ] 4.2 Normalize `TABLE_TYPE` the same way Postgres's `BASE TABLE` → `"TABLE"` mapping works (SQL
      Server's `INFORMATION_SCHEMA.TABLES.TABLE_TYPE` also returns `"BASE TABLE"`/`"VIEW"`).
- [ ] 4.3 Populate each `DatabaseObjectColumn`'s `TypeName` from `INFORMATION_SCHEMA.COLUMNS.DATA_TYPE`,
      and applicable `Attributes` (e.g. `Nullable` when `IS_NULLABLE = 'YES'`, `Max Length` from
      `CHARACTER_MAXIMUM_LENGTH` when present) — mirroring `PostgreSqlConnector.GetColumnAttributes`'s
      shape, adapted to `INFORMATION_SCHEMA` columns available identically on SQL Server.

## 5. Connection testing and write-access probing

- [ ] 5.1 Implement `IConnectionTest.TestConnectionAsync`: open the connection (honoring the configured
      `AuthenticationMode`) and execute `SELECT 1`, returning a `ConnectionTestResult` (`Success`,
      `Message`, `Elapsed`), catching exceptions into a failure result rather than throwing.
- [ ] 5.2 Implement `IWriteAccessProbe.HasWriteAccessAsync`: for each table from
      `INFORMATION_SCHEMA.TABLES`, check `HAS_PERMS_BY_NAME` for `INSERT`/`UPDATE`/`DELETE` permission;
      return `true` if any table grants any of those permissions, else `false` — mirroring
      `PostgreSqlConnector.HasWriteAccessAsync`'s logic with `HAS_PERMS_BY_NAME` in place of
      `has_table_privilege`.

## 6. Tests

- [ ] 6.1 Add `TheGrid.Tests.Connectors` coverage for connection-string building across all three
      `AuthenticationMode` values, including the `ConnectorParameterException` thrown when Service
      Principal mode is missing `TenantId`/`ClientId`/`ClientSecret`.
- [ ] 6.2 Add tests for `GetDataAsync` row/column mapping and `queryParameters` binding, using a SQL
      Server test fixture/container consistent with how other integration-style connector tests are set
      up in this repo (see `tests/fixtures/*.sql` conventions).
- [ ] 6.3 Add tests for `GetSchemaAsync`'s `INFORMATION_SCHEMA`-based table/view/column discovery and
      attribute population.
- [ ] 6.4 Add tests for `TestConnectionAsync` success/failure result shape and `HasWriteAccessAsync`
      true/false cases.
- [ ] 6.5 Confirm the discovery flags (`SupportsSchemaDiscovery`, `SupportsConnectionTest`,
      `SupportsWriteAccessProbe`) are all set correctly for this connector via
      `ConnectorDiscoveryService`.

## 7. Documentation

- [ ] 7.1 Write `docs/connectors/SQL Server.md` (filename keyed to the `[Connector("SQL Server", ...)]`
      display name, per the convention established in
      `openspec/changes/archive/2026-07-27-add-connector-user-documentation/design.md`), covering the
      standard six sections: Overview; Connection parameters (including all three `AuthenticationMode`
      values and which parameters apply to each); `Query.Command` contract (plain T-SQL,
      `queryParameters` binding via `SqlParameter`); Worked examples; Schema discovery
      (`INFORMATION_SCHEMA`-based, same shape as the PostgreSQL doc); Capabilities and caveats
      (`IConnectionTest`, `IWriteAccessProbe` via `HAS_PERMS_BY_NAME`, and a note that the UI shows all
      connection parameters flatly regardless of selected `AuthenticationMode` — a known rough edge).
      Written for query authors, not connector builders — no design-rationale prose (that stays in this
      change's own `design.md`).
- [ ] 7.2 Cross-check every parameter, behavior, and capability claim in the finished doc directly
      against `source/TheGrid.Connectors/SqlServerConnector.cs` (not from memory/paraphrase) before
      considering it done.
- [ ] 7.3 Update `docs/roadmap/ChangeSpecs.md`'s Phase 2+ section to mark the SQL Server connector item
      as done once implemented, consistent with how prior completed roadmap items are annotated.

## 8. Verify

- [ ] 8.1 Confirm the implementation satisfies every requirement/scenario in
      `openspec/changes/add-sqlserver-connector/specs/sqlserver-connector/spec.md`.
- [ ] 8.2 `dotnet build TheGrid.sln` and `dotnet test TheGrid.sln` pass with the new connector and its
      tests included.
