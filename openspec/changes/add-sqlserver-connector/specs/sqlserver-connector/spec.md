## ADDED Requirements

### Requirement: Query.Command is plain T-SQL text executed via Microsoft.Data.SqlClient
`Query.Command` for a SQL Server connection SHALL be plain T-SQL text, executed as-is against the
configured database via `Microsoft.Data.SqlClient` (not the legacy `System.Data.SqlClient`). No JSON
envelope or command-mode selection SHALL be required or supported.

#### Scenario: A SELECT query executes as-is
- **WHEN** a query's `Command` is `SELECT id, status FROM orders WHERE status = 'shipped'`
- **THEN** the connector executes that text directly against the configured database via
  `Microsoft.Data.SqlClient` and returns matching rows

### Requirement: Query parameters bind via SqlParameter
When a caller supplies `queryParameters` alongside a query's `Command`, the connector SHALL bind each
name/value pair as a `SqlParameter` using `@paramName`-style placeholders in the command text, rather
than string-concatenating values into the T-SQL text.

#### Scenario: A supplied parameter binds safely
- **WHEN** a query's `Command` is `SELECT * FROM orders WHERE status = @status` and `queryParameters`
  contains `{"status": "shipped"}`
- **THEN** the connector binds `@status` as a `SqlParameter` with value `"shipped"`, not by concatenating
  the value into the command text

### Requirement: Connection parameters follow the Postgres SQL-authentication baseline
The connector SHALL declare `ConnectionString`, `DatabaseName`, `Username`, `Password`, and
`PortNumber` connector parameters (using the corresponding `CommonConnectionParameters` keys), matching
the required/shape pattern `PostgreSqlConnector` uses for SQL-authentication connections.

#### Scenario: A SQL-authentication connection declares the baseline parameters
- **WHEN** the connector's declared parameters are inspected
- **THEN** `ConnectionString`, `DatabaseName`, `Username`, `Password`, and `PortNumber` are present,
  keyed by the same `CommonConnectionParameters` constants `PostgreSqlConnector` uses for
  `ConnectionString`/`DatabaseName`/`Username`/`Password`

### Requirement: AuthenticationMode selects among SQL Authentication, Azure AD Service Principal, and Azure AD Default Credential Chain
The connector SHALL declare an `AuthenticationMode` connector parameter selecting one of three
connection-establishment strategies: SQL Authentication (username/password), Azure AD Service Principal,
or Azure AD Default Credential Chain.

#### Scenario: SQL Authentication mode uses Username and Password
- **WHEN** a connection's `AuthenticationMode` is set to SQL Authentication
- **THEN** the connector opens the `SqlConnection` using the configured `Username`/`Password`, with no
  Azure AD token acquisition

#### Scenario: Service Principal mode acquires a token via ClientSecretCredential
- **WHEN** a connection's `AuthenticationMode` is set to Azure AD Service Principal, with valid
  `TenantId`, `ClientId`, and `ClientSecret` values configured
- **THEN** the connector constructs a `ClientSecretCredential` from those three values and sets
  `SqlConnection.AccessTokenCallback` to acquire a token for scope
  `https://database.windows.net/.default`, rather than embedding `Authentication=` connection-string
  keywords

#### Scenario: Default Credential Chain mode acquires a token via DefaultAzureCredential
- **WHEN** a connection's `AuthenticationMode` is set to Azure AD Default Credential Chain
- **THEN** the connector constructs a `DefaultAzureCredential` and sets
  `SqlConnection.AccessTokenCallback` to acquire a token for scope
  `https://database.windows.net/.default`, with no `TenantId`/`ClientId`/`ClientSecret` values required

### Requirement: Service Principal parameters are not required at the attribute level; the connector validates them itself
`TenantId`, `ClientId`, and `ClientSecret` SHALL be declared with `Required = false` at the
`[ConnectorParameter]` attribute level (`ClientSecret` marked `IsSecret = true`). The connector SHALL
independently validate, in its own code, that all three are present and non-empty whenever
`AuthenticationMode` is set to Azure AD Service Principal, throwing `ConnectorParameterException`
directly when they are not.

#### Scenario: A non-Service-Principal connection is not blocked by missing Azure AD parameters
- **WHEN** a connection's `AuthenticationMode` is SQL Authentication and `TenantId`/`ClientId`/
  `ClientSecret` are not set
- **THEN** the connection is not rejected by `ConnectorBase`'s required-parameter validation on account
  of those three parameters being absent

#### Scenario: A Service Principal connection missing a required Azure AD parameter fails clearly
- **WHEN** a connection's `AuthenticationMode` is Azure AD Service Principal and `ClientSecret` is not
  set
- **THEN** the connector throws a `ConnectorParameterException` identifying the missing parameter(s),
  rather than attempting to authenticate with incomplete credentials

### Requirement: Schema discovery queries INFORMATION_SCHEMA, not sys.* catalog views
`ISchemaDiscovery.GetSchemaAsync` SHALL populate one `DatabaseObject` per table or view by querying
`INFORMATION_SCHEMA.COLUMNS` joined to `INFORMATION_SCHEMA.TABLES`, mirroring
`PostgreSqlConnector.GetSchemaAsync`'s approach. It SHALL NOT query SQL Server's `sys.tables`/
`sys.columns` catalog views.

#### Scenario: Tables and views are discovered via INFORMATION_SCHEMA
- **WHEN** `GetSchemaAsync` runs against a database containing base tables and views
- **THEN** the returned `DatabaseSchema.DatabaseObjects` contains one `DatabaseObject` per table/view,
  built from `INFORMATION_SCHEMA.COLUMNS`/`INFORMATION_SCHEMA.TABLES` data, with base tables normalized
  to `ObjectTypeName == "TABLE"`

### Requirement: Write-access probing uses HAS_PERMS_BY_NAME
The connector SHALL implement `IWriteAccessProbe.HasWriteAccessAsync` by checking, for each table
discoverable via `INFORMATION_SCHEMA.TABLES`, whether the built-in `HAS_PERMS_BY_NAME` system function
reports `INSERT`, `UPDATE`, or `DELETE` permission for the current connection's identity. If any table
grants any of those permissions, the connection SHALL be reported as write-capable.

#### Scenario: A connection with write permission on any table is flagged write-capable
- **WHEN** `HAS_PERMS_BY_NAME` reports `INSERT`, `UPDATE`, or `DELETE` permission for at least one table
  reachable by the connection
- **THEN** `HasWriteAccessAsync` returns `true`

#### Scenario: A connection with no write permission on any table is not flagged write-capable
- **WHEN** `HAS_PERMS_BY_NAME` reports no `INSERT`, `UPDATE`, or `DELETE` permission for any table
  reachable by the connection
- **THEN** `HasWriteAccessAsync` returns `false`

### Requirement: Connectivity testing mirrors the Postgres pattern
The connector SHALL implement `IConnectionTest.TestConnectionAsync` by opening a `SqlConnection` (using
whichever `AuthenticationMode` is configured) and executing a trivial `SELECT 1` statement, returning a
`ConnectionTestResult` (`Success`, `Message`, `Elapsed`) per the existing capability-result contract.

#### Scenario: A reachable connection reports success
- **WHEN** `TestConnectionAsync` is called against a valid, reachable connection under any configured
  `AuthenticationMode`
- **THEN** it returns a `ConnectionTestResult` with `Success == true` and a non-negative `Elapsed`

#### Scenario: An unreachable or invalid connection reports failure as data
- **WHEN** `TestConnectionAsync` is called against an unreachable or invalid connection
- **THEN** it returns a `ConnectionTestResult` with `Success == false` and a non-null `Message`, rather
  than throwing an exception

### Requirement: Editor syntax highlighting uses the generic Sql language
The connector SHALL declare `EditorLanguage.Sql` as its `[Connector]` attribute's `EditorLanguage`. It
SHALL NOT introduce a new `Mssql`/`TSql`-specific `EditorLanguage` constant.

#### Scenario: The connector's editor language is the generic Sql constant
- **WHEN** the connector's `[Connector]` attribute is inspected
- **THEN** its `EditorLanguage` is `EditorLanguage.Sql`, not a SQL-Server-specific constant
