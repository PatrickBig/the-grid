## 1. ConnectorContext and package references

- [x] 1.1 Add `Microsoft.Extensions.Logging.Abstractions` and `Microsoft.Extensions.Http` package
      references to `TheGrid.Connectors.Abstractions.csproj` (lightweight framework abstractions — see
      design.md for why this doesn't violate the project's "no EF/Npgsql" bar).
- [x] 1.2 Add `ConnectorContext.cs` to `TheGrid.Connectors.Abstractions` (namespace `TheGrid.Connectors`,
      matching the project's existing namespace convention): a sealed record
      `ConnectorContext(Dictionary<string, string> Parameters, ILoggerFactory LoggerFactory,
      IHttpClientFactory HttpClientFactory)`.

## 2. ConnectorBase and shipped connectors

- [x] 2.1 Change `ConnectorBase`'s constructor from `ConnectorBase(Dictionary<string, string>
      connectorParameters)` to `ConnectorBase(ConnectorContext context)`. Set `ConnectorParameters` from
      `context.Parameters` (unchanged downstream behavior — `ValidateParameters` still operates on the
      same dictionary). Add `protected ILoggerFactory LoggerFactory { get; }` and `protected
      IHttpClientFactory HttpClientFactory { get; }`, both set from `context`.
- [x] 2.2 Update `PostgreSqlConnector`'s constructor: `public PostgreSqlConnector(ConnectorContext
      context) : base(context) { }`.
- [x] 2.3 Update `TestConnector`'s constructor (primary constructor style):
      `public class TestConnector(ConnectorContext context) : ConnectorBase(context)`.

## 3. IConnectorFactory

- [x] 3.1 Add `IConnectorFactory.cs` to `TheGrid.Services`: `IConnector Create(string connectorId,
      Dictionary<string, string> parameters)`.
- [x] 3.2 Add `ConnectorFactory.cs` to `TheGrid.Services` implementing `IConnectorFactory`, constructed
      with `ILoggerFactory`/`IHttpClientFactory` (primary constructor). `Create` resolves the type via
      `Assembly.GetAssembly(typeof(PostgreSqlConnector))` (same anchor pattern established in the
      `connectors-abstractions-split` change), builds a `ConnectorContext` from the injected factories
      plus the passed-in `parameters`, and calls `Activator.CreateInstance(type, context)`, throwing the
      same exceptions `QueryExecutor.GetConnector()` used to throw for a missing type or failed cast.

## 4. QueryExecutor

- [x] 4.1 Add `IConnectorFactory _connectorFactory` as a new constructor dependency on `QueryExecutor`
      (primary constructor — add to the existing parameter list).
- [x] 4.2 Rewrite `GetConnector(Query query)`: keep the existing logic that merges
      `ConnectionProperties` with decrypted `SecretProperties` into one dictionary, but convert it to
      `Dictionary<string, string>` via `.ToDictionary(kv => kv.Key, kv => kv.Value ?? string.Empty)`
      before calling `_connectorFactory.Create(query.Connection!.ConnectorId, parameters)`. Remove the
      `Assembly.GetAssembly`/`Activator.CreateInstance` calls entirely — that logic now lives in
      `ConnectorFactory`.

## 5. DI registration

- [x] 5.1 In `TheGrid.Services/DependencyInjection/TheGridContextServices.cs`'s
      `AddTheGridBackendServices`, add `services.AddHttpClient();` (currently registered nowhere
      server-side — only `TheGrid.Client/Program.cs` calls this, for the Blazor WASM client. Without
      this, `ConnectorFactory`'s `IHttpClientFactory` dependency fails to resolve at runtime).
- [x] 5.2 Register `services.AddTransient<IConnectorFactory, ConnectorFactory>();` in the same method.

## 6. Tests

- [x] 6.1 Add a small internal test helper in `TheGrid.Tests.Connectors` (e.g. a private/internal static
      method) that builds a `ConnectorContext` from just a `Dictionary<string, string>`, using
      `NullLoggerFactory.Instance` and a minimal fake/no-op `IHttpClientFactory` — see design.md for why
      this stays local to the test project rather than moving to `TheGrid.TestHelpers`.
- [x] 6.2 Update all `new PostgreSqlConnector(...)` / `new TestConnector(...)` call sites in
      `PostgreSqlConnectorTests.cs`, `TestConnectorTests.cs`, and `ConnectorExtensionsTests.cs` (~14
      found during grounding — grep to confirm none are missed) to use the new helper instead of passing
      a raw dictionary.
- [x] 6.3 Update all `new QueryExecutor(...)` call sites in `QueryExecutorTests.cs` (~8 found during
      grounding) to supply a mocked `IConnectorFactory` (NSubstitute) instead of relying on the real
      `Activator`-based resolution the tests previously exercised implicitly. Where a test's intent was
      actually to exercise real connector resolution (e.g. asserting `PostgreSqlConnector` is
      successfully instantiated by `ConnectorId` — added in the `connector-parameter-keys`/
      `connectors-abstractions-split` changes), configure the mock to return a real
      `PostgreSqlConnector`/`TestConnector` instance built via the task 6.1 helper, so that regression
      coverage isn't lost.

      Implementation note: rather than duplicating a `ConnectorContext`-builder across projects (the
      design explicitly scopes the task 6.1 helper to `TheGrid.Tests.Connectors` only, to avoid a new
      cross-project test dependency), all 9 call sites (one more than the ~8 estimate) use a local
      `CreateConnectorFactory()` helper that returns an NSubstitute `IConnectorFactory` whose `Create`
      forwards to a real `ConnectorFactory` (built from `NullLoggerFactory.Instance` and a substituted
      `IHttpClientFactory`). This preserves genuine connector construction/behavior (real row generation
      from `TestConnector`, a real failing connection attempt from `PostgreSqlConnector`) for every test,
      including the `ResolvesPostgreSqlConnectorByConnectorId` regression test, while still satisfying
      "supply a mocked `IConnectorFactory` (NSubstitute)".
- [x] 6.4 Add a unit test for `ConnectorFactory.Create`: resolves and constructs `PostgreSqlConnector`
      correctly by its full type name; throws for an unknown `connectorId`.
- [x] 6.5 Add a test confirming `ConnectorBase`-derived connectors expose non-null `LoggerFactory`/
      `HttpClientFactory` after construction.

## 7. Verification

- [x] 7.1 `dotnet build TheGrid.sln` (or per-project if the solution build hits the known pre-existing
      `docker-compose.dcproj`/`NU1105` issue — confirm unrelated) — no errors.
- [x] 7.2 Run the full test suite project-by-project — all green.
- [x] 7.3 Manual end-to-end check via Docker Compose ONLY (never `dotnet run` directly):
      `docker-compose -f source/docker-compose.yml -f source/docker-compose.override.yml up` from
      `source/`, create a connection and execute a query against it (this is the concrete proof that DI
      resolves `IConnectorFactory`/`IHttpClientFactory` correctly at runtime — a gap unit tests with
      mocks wouldn't catch), then `... down` immediately after. Do not leave the stack running.

      Result: brought the stack up with `-f docker-compose.yml -f docker-compose.override.yml` plus a
      third, untracked local-only override file (deleted before finishing; `git status`/`git diff` show
      zero changes to any docker-compose file) supplying a real `SecretProtectionOptions__EncryptionKey`
      to work around the known blank-key issue. Registered a new user via `/api/v1/account/register`,
      linked it to the pre-existing `acme` organization via a direct SQL insert into `UserOrganizations`
      (working around the known EF collection-navigation bug in `GroupManager.CreateGroupAsync` by not
      going through `OrganizationManager` at all), then via the real HTTP API: created a
      `PostgreSqlConnector` connection pointed at the `connector.postgres` compose service, created a
      query (`SELECT 1 as one, current_database() as db`), which auto-queued a Hangfire refresh job.
      The job executed for real and completed successfully (`QueryExecutions.Status = Complete`), and
      `QueryResultRows` contains the real row `{"one":1,"db":"connector"}` — proving `QueryExecutor` ->
      `IConnectorFactory` -> `ConnectorFactory` -> `PostgreSqlConnector` (built with a real
      `ILoggerFactory`/`IHttpClientFactory` from DI) resolved and ran correctly end-to-end. Server logs
      showed no DI resolution errors for `IConnectorFactory`/`IHttpClientFactory`/`ConnectorFactory`.
      Stack was brought down immediately after with `docker-compose ... down`.
