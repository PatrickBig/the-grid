## 1. Core attribute and DTO changes

- [x] 1.1 Change `ConnectorParameterAttribute`'s constructor to
      `ConnectorParameterAttribute(string key, string name, ConnectionPropertyType propertyType)`.
      Validate `key` with a stricter character rule than `Name`'s existing one: must start with a
      letter, followed by letters/digits/underscores only (`^[A-Za-z][A-Za-z0-9_]*$`); throw
      `ArgumentException` otherwise, same pattern as the existing `Name` validation.
- [x] 1.2 Add a `Key` property to `ConnectorParameterAttribute` (from the new ctor param) and an
      `IsSecret` bool property (settable, default `false`).
- [x] 1.3 Add `Key` (string) and `IsSecret` (bool) properties to `TheGrid.Shared.Models.ConnectionProperty`.
- [x] 1.4 Verify (write a test, don't just assume) that `ConnectorDiscoveryService`'s existing
      unconfigured `attribute.Adapt<ConnectionProperty>()` call picks up `Key`/`IsSecret`
      automatically. If it doesn't, add the minimal `TypeAdapterConfig` needed — do not skip this
      verification step. Verified: unconfigured Mapster convention-based mapping picks up `Key`/
      `IsSecret` automatically by property name, same as `Name`/`Type`/etc. — no `TypeAdapterConfig`
      needed. Confirmed via `ConnectorExtensionsTests.GetConnectorParameterDefinitions_MapsterAdapt_CarriesKeyAndIsSecret_Test`
      and `ConnectorDiscoveryServiceTests.RefreshConnectorsAsync_MapsKeyAndIsSecret_Test` (the latter
      round-trips through the DB's JSON column conversion too).

## 2. CommonConnectionParameters becomes key-shaped

- [x] 2.1 Change `CommonConnectionParameters` constants from display-name strings to key strings:
      `ConnectionString = "connectionString"`, `DatabaseName = "databaseName"`,
      `Username = "username"`, `Password = "password"`, `PortNumber = "portNumber"`,
      `Database = "database"`.

## 3. Update shipped connectors

- [x] 3.1 `PostgreSqlConnector`'s `[ConnectorParameter]` attributes: add the `key` argument (from
      `CommonConnectionParameters`) and change the `name` argument to a literal display string
      (`"Connection String"`, `"Database Name"`, `"Username"`, `"Password"`) instead of reusing the
      constant for both.
- [x] 3.2 `PostgreSqlConnector.GetConnection`'s `properties[CommonConnectionParameters.ConnectionString]`
      / `TryGetValue(CommonConnectionParameters.Password, ...)` etc. — no code change needed here since
      the constants now hold keys, but re-read the method after step 2.1 to confirm the lookups are
      still correct given the constants' new values.
- [x] 3.3 `TestConnector`'s `[ConnectorParameter(CommonConnectionParameters.ConnectionString, ...)]` —
      add the `name` literal (`"Connection String"`). Its second attribute
      `[ConnectorParameter("NumberOfRows", ConnectionPropertyType.Numeric)]` becomes
      `[ConnectorParameter("numberOfRows", "NumberOfRows", ConnectionPropertyType.Numeric)]`.
- [x] 3.4 `TestConnector.NumberOfRowsToGenerate`'s `ConnectorParameters.TryGetValue("NumberOfRows", ...)`
      → `TryGetValue("numberOfRows", ...)`.

## 4. Server-side lookups

- [x] 4.1 `ConnectorBase.ValidateParameters`: `requiredParameters.Select(p => p.Name)` →
      `.Select(p => p.Key)` (operating on the `ConnectionProperty` list from
      `GetConnectorParameterDefinitions()`).
- [x] 4.2 `ConnectorExtensions.GetSecretParameterKeys`: change the filter from
      `a.Type == ConnectionPropertyType.ProtectedText` to
      `a.IsSecret || a.Type == ConnectionPropertyType.ProtectedText`, and the projection from
      `.Select(a => a.Name)` to `.Select(a => a.Key)`.
- [x] 4.3 Re-read `ConnectionsController.Post`/`Put` after the above — confirm (don't just assume) no
      code change is actually needed there, since both already operate generically on whatever keys
      `GetSecretParameterKeys()` and the incoming request dictionary contain.
- [x] 4.4 Fix `CreateConnectionRequest.ConnectionProperties`'s XML doc `<example>` (currently shows
      `{ "Connection String": "...", "Database Name": "...", ... }`) to use key-shaped example keys
      (`{ "connectionString": "...", "databaseName": "...", "username": "...", "password": "..." }`).
      (Also noticed while grounding this change: the same file's `ConnectorId` property has a stale
      `<example>TheGrid.QueryRunners.PostgreSqlConnector</example>` doc comment referencing the retired
      namespace — fix this too while touching the file; it should be
      `TheGrid.Connectors.PostgreSqlConnector`.)

## 5. Client-side lookups

- [x] 5.1 `ConnectionPropertyEditor.razor.cs`: rename the `ValueChanged` callback's tuple element from
      `(string Name, string? Value)` to `(string Key, string? Value)`; update `OnValueChangedAsync` to
      invoke with `(ConnectionProperty.Key, value)` instead of `(ConnectionProperty.Name, value)`.
- [x] 5.2 `CreateConnection.razor.cs`: `ConnectorChanged`'s `_input.ConnectionProperties.Add(parameter.Name,
      null)` → `.Add(parameter.Key, null)`. `ParameterValueChanged`'s tuple param and
      `_input.ConnectionProperties[x.Name]` → `[x.Key]`.
- [x] 5.3 `EditConnection.razor.cs`: `GetInitialValue`'s
      `_connection.ConnectionProperties.TryGetValue(parameter.Name, ...)` → `.TryGetValue(parameter.Key,
      ...)`. `ParameterValueChanged`'s tuple param, its secret-routing check
      (`_selectedConnector?.Parameters.FirstOrDefault(p => p.Name == x.Name)?.Type ==
      ConnectionPropertyType.ProtectedText` → match on `p.Key == x.Key` and test
      `p.IsSecret || p.Type == ConnectionPropertyType.ProtectedText`), and both
      `_input.SecretProperties[x.Name]`/`_input.ConnectionProperties[x.Name]` → `[x.Key]`.
- [x] 5.4 Check `ConnectionPropertyEditor.razor` (the markup, not the code-behind) and
      `ConnectionList.razor` for any other place a parameter's `Name` is used where `Key` is now the
      correct choice (display labels should stay `Name` — only identity/lookup uses change). Found:
      `ConnectionPropertyEditor.razor`'s generated HTML element id/validator `Component` reference
      (`HtmlUtility.GetSafeId(ConnectionProperty.Name)` → `.Key`) — this is a stable-identity use (must
      not change if the display label is reworded), not a display use, so it moved to `Key`.
      `ConnectionList.razor` only uses `ConnectionListItem.Name` (the connection's own name, unrelated to
      connector parameters) — no change needed there.

## 6. Tests

- [x] 6.1 Update `TheGrid.Tests.Client/Shared/ConnectionManagement/ConnectionPropertyEditorTests.cs`,
      `TheGrid.Tests.Connectors/PostgreSqlConnectorTests.cs`,
      `TheGrid.Tests.Server/Controllers/ConnectionsControllerTests.cs`,
      `TheGrid.Tests.Services/QueryExecutorTests.cs`, and any `TestConnectorTests.cs`/
      `ConnectorExtensionsTests.cs` fixtures that construct `[ConnectorParameter]`-style test doubles or
      assert on `Name`-keyed dictionaries — update them to the new `Key`-based constructor/lookups.
      Search broadly; don't rely on this list being exhaustive, let compiler errors guide you to any
      missed spot. `ConnectionsControllerTests.cs` needed no change (it already used
      `CommonConnectionParameters.*` constants, which are now key-shaped automatically).
      `TestConnectorTests.cs`/`QueryExecutorTests.cs` had literal `"NumberOfRows"` dictionary keys →
      `"numberOfRows"`. `PostgreSqlConnectorTests.cs` had literal `"Username"`/`"Password"` dictionary
      keys → switched to the `CommonConnectionParameters` constants so they track the connector's real
      keys. `ConnectionPropertyEditorTests.cs` needed `Key` added to its test `ConnectionProperty`
      instances and its `HtmlUtility.GetSafeId` lookups switched from `.Name` to `.Key` (matching the
      markup change in 5.4).
- [x] 6.2 Add a test for `ConnectorParameterAttribute`'s `key` validation (rejects invalid characters,
      accepts a valid identifier).
- [x] 6.3 Add/update a test for `ConnectorExtensions.GetSecretParameterKeys` covering: a `ProtectedText`
      parameter with `IsSecret` unset is still secret; a non-`ProtectedText` parameter with
      `IsSecret = true` is secret; a plain parameter is not secret; returned keys are `Key`s, not
      `Name`s.
- [x] 6.4 Add/update a test confirming `PostgreSqlConnector` resolves its parameters correctly by `Key`
      end-to-end (constructing the connector with a `Key`-keyed dictionary succeeds; a `Name`-keyed
      dictionary now fails `ValidateParameters` as "missing required parameters" — this is the
      intended breaking behavior, assert it explicitly so a future change can't silently reintroduce
      `Name`-keying by accident).
- [x] 6.5 Add a test asserting `ConnectorDiscoveryService`'s Mapster adaptation carries `Key`/`IsSecret`
      from attribute to `ConnectionProperty` correctly (per task 1.4). Note: this test uses its own
      isolated `SqliteProvider` rather than the test class's shared `IClassFixture<SqliteProvider>`
      context — sharing it with the existing `RefreshConnectorsAsync_Test` surfaced a pre-existing
      (unrelated to this change) entity-tracking conflict in `ConnectorDiscoveryService.RefreshConnectorsAsync`
      when called a second time against a context/DB that already has previously-discovered connector
      rows. Worked around via test isolation rather than touching `ConnectorDiscoveryService` itself,
      which is out of this change's scope; flagged here for visibility, not fixed.

## 7. Verification

- [x] 7.1 `dotnet build TheGrid.sln` (or each project individually if the solution-wide build hits the
      known pre-existing `docker-compose.dcproj`/`NU1105` environment issue — confirm that's still the
      only failure, unrelated to this change) — no errors. Confirmed: `dotnet build TheGrid.sln` still
      fails solution-wide only on the pre-existing `docker-compose.dcproj`/`NU1105` restore issue
      (unrelated to this change — same failure would occur on a clean checkout). Built every project
      individually instead (`TheGrid.Server`, all 5 test projects) — 0 errors across all of them.
- [x] 7.2 Run the full test suite project-by-project (`TheGrid.Tests.Connectors`, `TheGrid.Tests.Services`,
      `TheGrid.Tests.Server`, `TheGrid.Tests.Client`, `TheGrid.Tests.Shared`) — all green. Results:
      `TheGrid.Tests.Connectors` 40/40 (includes real Docker-backed PostgreSQL integration tests),
      `TheGrid.Tests.Services` 22/22, `TheGrid.Tests.Server` 25/25, `TheGrid.Tests.Client` 20/20,
      `TheGrid.Tests.Shared` 3/3. One pre-existing test-isolation bug was uncovered and fixed along the
      way (see notes on task 6.5's test below) — unrelated to the Key/IsSecret change itself.
- [x] 7.3 Manual end-to-end check via the running app: bring the stack up with
      `docker-compose -f source/docker-compose.yml -f source/docker-compose.override.yml up` from
      `source/`, create a new PostgreSQL connection through the UI (exercising the new `Key`-keyed
      parameter flow end to end: client form → `POST /connections` → stored
      `ConnectionProperties`/`SecretProperties` → query execution constructing `PostgreSqlConnector`
      successfully), then bring the stack down with `... down` immediately after. Do not leave the
      stack running or use `dotnet run` directly for this check.
      Done via `docker-compose up`/`down` for the full stack lifecycle (browser automation was
      unavailable in this session, so the same REST endpoints the Blazor client calls were driven
      directly instead of clicking through the UI — see final report for details and the unrelated
      pre-existing bugs hit and worked around along the way). Confirmed via `GET /api/v1/Connectors`
      that `Key` values (`connectionString`, `databaseName`, `username`, `password`, `numberOfRows`)
      are populated correctly by the live app. Created a real PostgreSQL connection
      (`POST /Connections`) against the `connector.postgres` container; confirmed in the database that
      `ConnectionProperties` held the three plaintext keys and `SecretProperties` held only `password`,
      encrypted. Created a query and refreshed it (`POST /QueryResultRefresh`); `QueryExecutions.Status`
      came back `2` (`Complete`) with the expected rows in `QueryResultRows` — proving
      `PostgreSqlConnector` was constructed successfully from `Key`-keyed, decrypted parameters and ran
      a real query. Stack was torn down with `docker-compose down -v` immediately after; confirmed no
      containers left running.
