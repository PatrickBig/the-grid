# Current-State Reference (ground truth for implementers)

_Purpose: a verified snapshot of how things actually work today, with real file paths, so an
implementer (or a cheaper coding model) does not have to re-derive contracts and can avoid the known
traps. Pair this with `docs/Roadmap.md` (the "why") — this doc is the "what is." **If code and this
doc disagree, the code wins; update this doc.**_

_Verified: 2026-07-25 against branch `feature/group-and-permission-manager`._

---

## Connector SDK — as-built contracts

### Core types (`source/TheGrid.Connectors/`)

| Type | File | Current signature / shape |
|---|---|---|
| `IConnector` | `IConnector.cs` | `IAsyncEnumerable<ConnectorRow> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, [EnumeratorCancellation] CancellationToken ct = default)` — **streams rows, does not buffer the result set** |
| `ConnectorBase` | `ConnectorBase.cs` | abstract; ctor takes `Dictionary<string,string> connectorParameters`, stores as `protected Dictionary<string,string> ConnectorParameters`, calls `ValidateParameters` |
| `ConnectorAttribute` | `Attributes/ConnectorAttribute.cs` | `[Connector(string name)]` + `EditorLanguage`, `IconFileName` |
| `ConnectorParameterAttribute` | `Attributes/ConnectorParameterAttribute.cs` | `[ConnectorParameter(string name, ConnectionPropertyType type)]` + `RenderOrder`, `HelpText` (≤200 chars), `Required`. **`AllowMultiple = true`** |
| `ISchemaDiscovery` | `ISchemaDiscovery.cs` | `Task<DatabaseSchema> GetSchemaAsync(ct)` |
| `IConnectionTest` | `IConnectionTest.cs` | `Task<bool> TestConnectionAsync(ct)` — **returns bare bool** |
| `IPermissionTest` | `IPermissionTest.cs` | `Task<bool> HasWritePermissionAsync(ct)` — write-access probe |
| `ConnectorRow` | `TheGrid.Shared/Models/ConnectorRow.cs` | `record ConnectorRow(IReadOnlyDictionary<string,QueryResultColumn> Columns, IReadOnlyDictionary<string,object?> Data)` — one streamed row; the same `Columns` reference is attached to every row in a stream, built once from the first row |
| `QueryResultColumn` | `TheGrid.Shared/Models/QueryResultColumn.cs` | just `QueryResultColumnType Type` |
| `QueryResultColumnType` (enum) | same file | `Text, Boolean, Integer, Long, Decimal, DateTime, Time` |

The buffered `QueryResult` envelope (`Columns`/`Rows`/`StandardOutput`) that `GetDataAsync` used to return has been **removed** — there is no buffered fallback path; both connectors stream directly.

### Concrete connectors
- `PostgreSqlConnector.cs` — implements `ISchemaDiscovery, IConnectionTest, IPermissionTest`. Uses Npgsql. Builds connection via `NpgsqlConnectionStringBuilder` from the `"Connection String"` param, then overrides `Password`/`Username`/`Database` from their own params.
- `TestConnector.cs` — `[ExcludeFromCodeCoverage]`, generates fake rows; special `ThrowExceptionQuery` const to force errors in tests. **Excluded from availability by a special rule — do not remove that exclusion.**

### ⚠️ Traps an implementer must know
1. **Parameters are keyed by DISPLAY NAME.** `ConnectorParameters["Connection String"]`. The constants in `CommonConnectionParameters.cs` are the *display strings themselves* (`ConnectionString = "Connection String"`). There is **no** separate machine key. Changing a label is a breaking data change. (Roadmap §3.2 fixes this by adding `Key`.)
2. **No secret flagging.** Nothing marks a param as a secret. `ConnectionPropertyType.ProtectedText` only changes the UI control; it does **not** drive encryption or redaction.
3. **Discovery is single-assembly.** `ConnectorDiscoveryService.GetConnectorTypes()` (`source/TheGrid.Services/ConnectorDiscoveryService.cs`) does `Assembly.GetAssembly(typeof(IConnector))` and scans only that assembly. Connectors in other assemblies are invisible.
4. **Instantiation is raw reflection.** `QueryExecutor.GetConnector()` does `Activator.CreateInstance(connectorType, query.Connection.ConnectionProperties)`. No DI, no logger/HttpClient injection. `ConnectorId` is the type's `FullName`.
5. **`GetQueryResultColumnTypeForType`** (`Extensions/TypeExtensions.cs`) has a **duplicate `long` branch and an unreachable `uint` arm** — fix carefully, it's easy to reintroduce.

---

## Persistence & secrets

- `Connection.ConnectionProperties` is `Dictionary<string,string?>`, mapped in
  `TheGridDbContext.OnModelCreating` via `HasConversion<JsonColumnConverter<...>>` →
  **plain JSON, NOT encrypted** despite the XML doc comment on the property claiming otherwise.
- `JsonColumnConverter<T>` (`source/TheGrid.Data/JsonColumnConverter.cs`) is generic plain
  `System.Text.Json` serialize/deserialize, reused for several columns (`QueryResultRow.Data`,
  `Connector.Parameters`, `TableVisualization.Columns`, `Connection.ConnectionProperties`). **Do not
  bolt encryption into this shared converter** — it would encrypt non-secret columns too. Introduce a
  dedicated secret-aware path instead.
- DB providers: Postgres (`Npgsql`) and Sqlite, selected by `SystemOptions.DatabaseProvider`; migrations
  live in provider-specific assemblies (`TheGrid.Postgres`, `TheGrid.Sqlite`). **Any schema change =
  a migration in each provider assembly** (see `docs/AddingMigrations.md`).

---

## Query execution path

`QueryExecutor.RefreshQueryResultsAsync(long queryExecutionId, ct)`
(`source/TheGrid.Services/QueryExecutor.cs`), `[Queue(JobQueues.QueryRefresh)]` (Hangfire):

1. Loads `QueryExecution` + `Query` + `Connection` + `Query.Columns`.
2. Sets status `InProgress`, saves.
3. Creates a linked `CancellationTokenSource` (`CancelAfter(ExecutionLimits.TimeoutSeconds)`) wrapping the caller's token.
4. Builds connector, `await foreach`s `GetDataAsync(query.Command, null, linkedCt)` — **`queryParameters` is always `null`**.
5. Adds each row as a `QueryResultRow`. Stops (without erroring) once `ExecutionLimits.MaxRows` is reached and sets `QueryExecution.Truncated = true`.
6. Calls `SaveChangesAsync` + `ChangeTracker.Clear()` every `ExecutionLimits.BatchSize` rows to bound memory, then **re-attaches `queryExecution`** (`_db.Attach(queryExecution)`) — `ChangeTracker.Clear()` detaches the whole graph including `queryExecution`/`Query`/`Query.Columns`, so skipping the re-attach silently drops the final `Truncated`/`Status`/column-definition updates.
7. `UpdateColumnDefinitions` reconciles `Query.Columns` against the first row's `Columns` (empty dictionary if the result set had zero rows).
8. Notifies clients via SignalR `IQueryDesignerHub.QueryResultsFinishedProcessing`.
9. On timeout (linked source fired, not the caller's own token): status `TimedOut`, no rethrow — rows persisted before the timeout are kept. On any other exception: status `Error`, stores `ex.Message`, rethrows. `finally` saves.

Execution limits (`MaxRows`, `TimeoutSeconds`, `BatchSize`) come from `SystemOptions.ExecutionLimits` (`TheGrid.Models/Configuration/SystemOptions.cs`), system-wide only — no per-query override yet. (Roadmap §4.3, implemented.)

---

## Authorization model (for feature work that touches access)

- `ApplicationPermission` enum (`TheGrid.Shared/Constants/ApplicationPermission.cs`) — fine-grained
  perms. **Note:** enum names are `ApproveConnections`/`ApproveQueries`/`ViewConnection` etc.; the
  `docs/Permissions.md` headings use slightly different singular/plural spellings — trust the enum.
- Perms attach to a `Group` via `GroupPermission`; users get them through `UserGroup` membership.
- Built-in system groups have `OrganizationId == null` and use **separate** `GroupManager` methods
  (`CreateSystemAdministratorGroupAsync`) — do not route them through `CreateGroupAsync`.
- Two enforcement mechanisms:
  - **Org scope:** `OrganizationHandler`/`OrganizationRequirement` compares the
    `ApplicationHeaders.OrganizationId` request header to the caller's `GridClaimTypes.Organization`
    claim (or `IsSystemAdministrator()`).
  - **Resource scope:** `ConnectionAuthorizationHandler` (`AuthorizationHandler<OperationAuthorizationRequirement, Connection>`)
    against `GridOperations.Read`/`Create`.

---

## Client / UI (Blazor WASM)

- Radzen.Blazor + BlazorMonaco (editor) + Blazored LocalStorage/SessionStorage.
- **Connection form is metadata-driven:** `Shared/ConnectionManagement/ConnectionPropertyEditor.razor`
  renders inputs from `ConnectionProperty` metadata. Enriching the parameter model (Roadmap §3.2)
  directly upgrades this screen.
- Visualizations: `Shared/Visualizations/Table.razor` is real; `Chart.razor` is a stub.
- No dashboard UI exists.

---

## Missing entities (named in permissions, not yet modeled)
`Dashboard`, `Alert`, `Folder` — permissions exist (`CreateDashboard`, `CreateAlert`, `ManageFolders`,
…) but there are **no** corresponding EF entities, managers, controllers, or UI. Greenfield.
