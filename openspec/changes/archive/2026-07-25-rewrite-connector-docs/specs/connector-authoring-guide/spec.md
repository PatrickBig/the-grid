## ADDED Requirements

### Requirement: Documented base class and attributes match current code
`docs/Creating-Connectors.md` SHALL describe connector authoring using the actual current types:
`TheGrid.Connectors.ConnectorBase` as the base class, `TheGrid.Connectors.Attributes.ConnectorAttribute`
applied to the class, and `TheGrid.Connectors.Attributes.ConnectorParameterAttribute` (one per
connection parameter, `AllowMultiple = true`) — not the retired `QueryRunner`/`QueryRunnerBase`/
`QueryRunnerAttribute`/`QueryRunnerParameterAttribute` names.

#### Scenario: Reader follows the doc to declare a connector class
- **WHEN** a reader follows the doc's guidance to declare a new connector class
- **THEN** the resulting class declaration (base class + attributes) compiles against the current
  `TheGrid.Connectors` assembly

### Requirement: Documented GetDataAsync signature matches the streaming contract
`docs/Creating-Connectors.md` SHALL document `GetDataAsync` as an
`IAsyncEnumerable<TheGrid.Shared.Models.ConnectorRow> GetDataAsync(string query,
Dictionary<string, object?>? queryParameters, CancellationToken cancellationToken = default)` method
implemented with `yield return` per row, not as a method returning a buffered `Task<QueryResult>`.

#### Scenario: Reader implements GetDataAsync per the doc
- **WHEN** a reader implements `GetDataAsync` following the doc's guidance
- **THEN** the resulting method signature matches `ConnectorBase.GetDataAsync`'s abstract signature and
  compiles as an override

### Requirement: Worked example references a real, current connector
`docs/Creating-Connectors.md` SHALL use `TheGrid.Connectors.PostgreSqlConnector` (or another connector
that exists in the codebase at doc-authoring time) as its primary worked example, quoting real excerpts
from the file rather than an invented class, so the example cannot silently drift from compiling code
without the referenced file itself changing.

#### Scenario: Worked example code matches the referenced source file
- **WHEN** a code excerpt in the doc is compared against the connector file it's presented as being
  from
- **THEN** the excerpt is a verbatim (or clearly-marked-abridged) subset of that file's actual contents

### Requirement: Current parameter-keying behavior is documented as-is
`docs/Creating-Connectors.md` SHALL document that connector parameters are currently keyed by the
`ConnectorParameterAttribute`'s display `Name` string (there is no separate machine key yet), as an
explicit current-behavior note — not omitted, and not described as already fixed — reflecting
`docs/architecture/CurrentState.md`'s trap #1 until a future change (`P1-1`) introduces a stable `Key`.

#### Scenario: Doc explains how a connector's stored parameters are keyed
- **WHEN** a reader looks for how `ConnectorParameters` dictionary keys relate to the attribute's `Name`
- **THEN** the doc states they are the same string, and that renaming a parameter's display `Name` is a
  breaking change to already-stored connections

### Requirement: Capability interfaces documented with accurate signatures
`docs/Creating-Connectors.md` SHALL document `ISchemaDiscovery.GetSchemaAsync`,
`IConnectionTest.TestConnectionAsync` (returning `Task<bool>`), and `IPermissionTest.HasWritePermissionAsync`
(returning `Task<bool>`) as opt-in capability interfaces a connector may additionally implement, with
their actual current method signatures.

#### Scenario: Reader adds a capability interface per the doc
- **WHEN** a reader implements one of the documented capability interfaces on their connector class
- **THEN** the interface member signature they implement matches the real interface definition in
  `TheGrid.Connectors`
