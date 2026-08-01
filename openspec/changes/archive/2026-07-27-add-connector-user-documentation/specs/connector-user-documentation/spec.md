## ADDED Requirements

### Requirement: Every shipped connector has a user-facing usage doc
Each connector class in `TheGrid.Connectors` decorated with `[Connector]` and intended for real user
data sources (i.e. excluding internal test-only connectors such as `TestConnector`) SHALL have a
corresponding user-facing usage doc at `docs/connectors/<ConnectorName>.md`, where `<ConnectorName>` is
the connector's `[Connector("Name", ...)]` display name, not its C# class name.

#### Scenario: A shipped connector's doc file exists under the expected name
- **WHEN** a connector is declared as `[Connector("PostgreSQL", ...)]`
- **THEN** a doc exists at `docs/connectors/PostgreSQL.md`

#### Scenario: A second shipped connector follows the same naming convention
- **WHEN** a connector is declared as `[Connector("MongoDB", ...)]`
- **THEN** a doc exists at `docs/connectors/MongoDB.md`

### Requirement: Doc audience is a query author, not a connector author
`docs/connectors/<ConnectorName>.md` SHALL be written for someone writing queries against an existing
connection using that connector — its connection parameters, its `Query.Command` contract, and how to
read its schema-discovery output — not for someone implementing the connector itself (that audience is
served by `docs/Creating-Connectors.md`), and it SHALL NOT narrate the design rationale behind a
specific implementation change (that belongs in that change's own OpenSpec `design.md`).

#### Scenario: A connector doc omits implementation-rationale content
- **WHEN** a connector's `docs/connectors/<ConnectorName>.md` is reviewed
- **THEN** it does not contain prose explaining why an internal design choice was made (e.g. why one
  JSON key shape was chosen over an alternative), only what the current contract is and how to use it

#### Scenario: A connector doc omits connector-building guidance
- **WHEN** a connector's `docs/connectors/<ConnectorName>.md` is reviewed
- **THEN** it does not explain how to implement `ConnectorBase`, `[ConnectorParameter]`, or capability
  interfaces — that guidance lives only in `docs/Creating-Connectors.md`

### Requirement: Doc covers a standard set of self-contained sections
`docs/connectors/<ConnectorName>.md` SHALL include, as clearly headed, self-contained sections: an
overview of what the connector connects to; its connection parameters and their meaning; its
`Query.Command` contract (query language or JSON shape accepted by `GetDataAsync`); at least one worked
example per distinct execution mode the connector supports; how schema discovery works for the connector
and how to interpret its output (if the connector implements `ISchemaDiscovery`); and the connector's
supported capability interfaces and any notable caveats or gaps.

#### Scenario: A connector with one execution mode has one worked example
- **WHEN** a connector's `Query.Command` accepts only one kind of input (e.g. a single SQL dialect)
- **THEN** its doc includes at least one worked example demonstrating that mode

#### Scenario: A connector with multiple execution modes has an example per mode
- **WHEN** a connector's `Query.Command` supports more than one distinct execution mode (e.g. find vs.
  aggregate)
- **THEN** its doc includes at least one worked example for each distinct mode

#### Scenario: A connector without schema discovery omits that section's applicability
- **WHEN** a connector does not implement `ISchemaDiscovery`
- **THEN** its doc's schema discovery section states that schema discovery is not supported for this
  connector, rather than omitting the section without explanation or describing discovery behavior that
  does not exist

### Requirement: Doc structure is self-contained enough to back future in-app help
Each section within `docs/connectors/<ConnectorName>.md` SHALL be self-contained under its own heading
(understandable without requiring the reader to have read prior sections first), so that the document
could plausibly be split into individually-rendered panels for in-app help without restructuring —
without this change designing, building, or committing to any actual in-app rendering or API surface for
that purpose.

#### Scenario: A section is readable in isolation
- **WHEN** a single section (e.g. "Connection parameters") of a connector doc is read on its own, without
  the surrounding sections
- **THEN** the section is understandable on its own terms, without unresolved references to content that
  only appears in a different section

### Requirement: Content stays synchronized with the connector's actual current behavior
A connector's `docs/connectors/<ConnectorName>.md` SHALL accurately describe that connector's actual
current connection parameters, `Query.Command` contract, schema-discovery behavior, and implemented
capability interfaces, kept up to date per the connector-documentation obligation already established in
CLAUDE.md's Documentation section (create or update the doc in the same change whenever a connector is
added or its query/schema-discovery behavior changes materially).

#### Scenario: A documented connection parameter matches the connector's declared parameters
- **WHEN** a connector doc's "Connection parameters" section is compared against that connector class's
  `[ConnectorParameter]` attributes
- **THEN** every required and optional parameter declared on the class is documented, with no documented
  parameter that the class does not actually declare

#### Scenario: A documented capability interface matches what the connector implements
- **WHEN** a connector doc states that a capability (e.g. `IWriteAccessProbe`) is or is not implemented
- **THEN** that statement matches whether the connector class actually implements that interface
