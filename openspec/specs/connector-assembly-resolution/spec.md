# connector-assembly-resolution Specification

## Purpose
TBD - created by archiving change connectors-abstractions-split. Update Purpose after archive.
## Requirements
### Requirement: Connector-related assembly resolution targets the concrete implementations assembly
Any code that reflects over "the assembly containing connector implementations" (for discovery,
instantiation by `ConnectorId`, or resolving secret parameter keys) SHALL anchor that resolution on a
type guaranteed to reside in the assembly containing concrete connector classes (e.g.
`PostgreSqlConnector`), not on a type that resides only in an interfaces/attributes/models assembly.

#### Scenario: Connector discovery finds all concrete connectors
- **WHEN** `ConnectorDiscoveryService.RefreshConnectorsAsync()` runs
- **THEN** it discovers both `PostgreSqlConnector` and `TestConnector`

#### Scenario: Query execution resolves a connector by ConnectorId
- **WHEN** `QueryExecutor` executes a query against a connection whose `ConnectorId` is
  `PostgreSqlConnector`'s full type name
- **THEN** it successfully locates and instantiates the `PostgreSqlConnector` type

#### Scenario: Connection creation resolves secret parameter keys
- **WHEN** a connection is created for a connector by its `ConnectorId`
- **THEN** the system successfully resolves that connector's declared secret parameter keys without
  throwing "no connector found"

