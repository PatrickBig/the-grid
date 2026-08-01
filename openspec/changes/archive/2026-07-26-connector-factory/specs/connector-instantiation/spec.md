## ADDED Requirements

### Requirement: Connectors are constructed via IConnectorFactory, not raw reflection
The system SHALL construct connector instances through an `IConnectorFactory` abstraction rather than
calling `Activator.CreateInstance` directly against a reflected type.

#### Scenario: Query execution constructs a connector through the factory
- **WHEN** `QueryExecutor` needs a connector instance to execute a query
- **THEN** it obtains that instance via `IConnectorFactory.Create`, not via its own
  `Activator.CreateInstance` call

#### Scenario: An unknown ConnectorId still fails clearly
- **WHEN** `IConnectorFactory.Create` is called with a `connectorId` that does not match any known
  connector type
- **THEN** it throws an exception indicating no connector was found, matching prior behavior

### Requirement: Connectors receive shared infrastructure through their constructor
A connector's base constructor SHALL receive an `ILoggerFactory` and an `IHttpClientFactory` alongside
its connection parameters, so a connector implementation can use logging and make outbound HTTP calls
without managing those resources itself.

#### Scenario: A connector can access an ILoggerFactory
- **WHEN** a connector inheriting `ConnectorBase` is constructed
- **THEN** it has access to an `ILoggerFactory` via a protected member on the base class

#### Scenario: A connector can access an IHttpClientFactory
- **WHEN** a connector inheriting `ConnectorBase` is constructed
- **THEN** it has access to an `IHttpClientFactory` via a protected member on the base class

### Requirement: Connector identity is preserved across the construction change
Changing how a connector is instantiated SHALL NOT change a connector's identity (`ConnectorId`, equal
to its type's full name) — existing `Connection` rows referencing a connector by that ID SHALL continue
to resolve correctly.

#### Scenario: An existing connection still resolves its connector
- **WHEN** a `Connection` whose `ConnectorId` was stored before this change is used to execute a query
- **THEN** `IConnectorFactory.Create` still successfully resolves and constructs that same connector type
