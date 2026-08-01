## Why

Every connector today must live inside `TheGrid.Connectors` itself, which references `Npgsql`,
`Dapper`, and `Mapster`. A third-party (or future in-house) connector author writing, say, a MySQL
connector has no way to depend on just "the connector contract" — they'd either have to add their code
to this project directly, or take on this project's own dependencies transitively. `docs/roadmap/ChangeSpecs.md`'s
P1-2 calls for splitting the interfaces/attributes/models into a dependency-light
`TheGrid.Connectors.Abstractions` project that concrete connector projects (including this one) reference,
without pulling in driver-specific packages.

## What Changes

- **New project** `TheGrid.Connectors.Abstractions` (net8.0, `Nullable`/`ImplicitUsings` enabled to match
  the rest of the solution). Contains only interfaces, attributes, models, and dependency-light
  extension logic — no `Npgsql`. Package references: `Mapster` only (needed by the parameter-metadata
  extension methods that move here — see below).
- **Files moved from `TheGrid.Connectors` to `TheGrid.Connectors.Abstractions`**, namespaces
  **unchanged** (see Decisions): `IConnector.cs`, `ConnectorBase.cs`, `ConnectorParameterException.cs`,
  `Attributes/ConnectorAttribute.cs`, `Attributes/ConnectorParameterAttribute.cs`, `ISchemaDiscovery.cs`,
  `IConnectionTest.cs`, `IPermissionTest.cs`, `Models/DatabaseObject.cs`, `Models/DatabaseObjectColumn.cs`,
  `Models/DatabaseSchema.cs`, `CommonConnectionParameters.cs`, `Extensions/TypeExtensions.cs`,
  `Extensions/ConnectorExtensions.cs`.
- **Stays in `TheGrid.Connectors`** (now a thin concrete-implementations project referencing
  Abstractions): `PostgreSqlConnector.cs`, `TestConnector.cs`. Its `Npgsql` package reference stays; its
  unused `Dapper` reference is removed (dead dependency, noticed during this restructuring — not
  something used anywhere in the project); its `Mapster` reference is removed (only
  `Extensions/ConnectorExtensions.cs`, which is moving out, used it).
- **BREAKING (correctness-critical, not just structural)**: `ConnectorDiscoveryService.GetConnectorTypes()`,
  `QueryExecutor.GetConnector()`, and `ConnectionsController.ResolveConnectorType()` all currently resolve
  "the assembly containing connectors" via `Assembly.GetAssembly(typeof(IConnector))`. Once `IConnector`
  moves to `TheGrid.Connectors.Abstractions`, that call would resolve to the *interfaces-only* assembly —
  which contains zero concrete connectors — silently breaking connector discovery, query execution, and
  connection creation everywhere. All three call sites must switch their anchor type to one that's
  guaranteed to live in the concrete `TheGrid.Connectors` assembly. See Decisions for the chosen anchor.
- `TheGrid.Services.csproj` and `TheGrid.Server.csproj` gain explicit `ProjectReference`s to
  `TheGrid.Connectors.Abstractions.csproj` (for the interface/attribute types they use directly), in
  addition to their existing (direct or transitive) reference to concrete `TheGrid.Connectors`.
- No behavior change from a user's perspective — this is a pure project-structure change. Solution
  builds, all tests pass, connector discovery/execution/creation work identically to before.

## Capabilities

### New Capabilities
- `connector-assembly-resolution`: the system SHALL correctly resolve the assembly containing concrete
  connector implementations for discovery, execution, and secret-key resolution — this is the one
  genuinely testable behavioral guarantee this otherwise-structural change must not break (see Why:
  three call sites currently anchor on `typeof(IConnector)`, which becomes wrong once interfaces move
  to a separate assembly).

### Modified Capabilities
(none — no existing capability's requirements change; connector discovery/execution/creation must
behave identically to before, just implemented via a corrected assembly anchor)

## Impact

- New project: `source/TheGrid.Connectors.Abstractions/TheGrid.Connectors.Abstractions.csproj`, added to
  `TheGrid.sln`.
- `source/TheGrid.Connectors/TheGrid.Connectors.csproj` — remove `Dapper`/`Mapster` package references,
  add `ProjectReference` to `TheGrid.Connectors.Abstractions`.
- `source/TheGrid.Services/ConnectorDiscoveryService.cs`, `QueryExecutor.cs` — anchor-type fix (see
  Decisions).
- `source/TheGrid.Server/Controllers/ConnectionsController.cs` — anchor-type fix (see Decisions).
- `source/TheGrid.Services/TheGrid.Services.csproj`, `source/TheGrid.Server/TheGrid.Server.csproj` —
  add explicit `ProjectReference` to `TheGrid.Connectors.Abstractions.csproj`.
- Test project: `source/tests/TheGrid.Tests.Connectors/TheGrid.Tests.Connectors.csproj` gains a
  `ProjectReference` to the new Abstractions project (kept as one test project — see design.md for why
  a separate `TheGrid.Tests.Connectors.Abstractions` project isn't created for this change).
- No EF migration, no data changes, no API contract changes.
