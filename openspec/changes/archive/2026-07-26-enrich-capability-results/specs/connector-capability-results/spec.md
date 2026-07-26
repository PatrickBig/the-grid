## ADDED Requirements

### Requirement: Connection tests report a structured result, not a bare boolean
`IConnectionTest.TestConnectionAsync` SHALL return a `ConnectionTestResult` (`Success: bool`,
`Message: string?`, `Elapsed: TimeSpan`) describing the outcome of the test, including a failure reason
when the test fails, rather than a bare `bool`.

#### Scenario: A successful connection test reports success with elapsed time
- **WHEN** `TestConnectionAsync` is called against a valid, reachable connection
- **THEN** it returns a `ConnectionTestResult` with `Success == true` and a non-negative `Elapsed`

#### Scenario: A failed connection test reports failure as data, not an exception
- **WHEN** `TestConnectionAsync` is called against an unreachable or invalid connection
- **THEN** it returns a `ConnectionTestResult` with `Success == false` and a non-null `Message`
  describing the failure, rather than throwing an exception

### Requirement: Write-access probing is named for what it checks
The capability interface for checking whether a connection's credentials have write access SHALL be
named `IWriteAccessProbe`, with a method `HasWriteAccessAsync`, rather than the previous
`IPermissionTest`/`HasWritePermissionAsync` naming.

#### Scenario: A connector implements the renamed interface
- **WHEN** a connector class implements write-access probing
- **THEN** it does so via `IWriteAccessProbe.HasWriteAccessAsync`, not `IPermissionTest.HasWritePermissionAsync`

### Requirement: Discovery-flag parity across capability interfaces
`ConnectorDiscoveryService` SHALL record a discovery-time flag on `Connector` for each of the three
capability interfaces (`ISchemaDiscovery` → `SupportsSchemaDiscovery`, `IConnectionTest` →
`SupportsConnectionTest`, `IWriteAccessProbe` → `SupportsWriteAccessProbe`), rather than only the first
two.

#### Scenario: A connector implementing IWriteAccessProbe is flagged as supporting it
- **WHEN** `ConnectorDiscoveryService.RefreshConnectorsAsync()` runs for a connector type implementing
  `IWriteAccessProbe`
- **THEN** that connector's `Connector.SupportsWriteAccessProbe` is `true`

#### Scenario: A connector not implementing IWriteAccessProbe is not flagged
- **WHEN** `ConnectorDiscoveryService.RefreshConnectorsAsync()` runs for a connector type not
  implementing `IWriteAccessProbe`
- **THEN** that connector's `Connector.SupportsWriteAccessProbe` is `false`
