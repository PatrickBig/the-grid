## 1. Ground the rewrite in current code

- [x] 1.1 Re-read `TheGrid.Connectors/ConnectorBase.cs`, `Attributes/ConnectorAttribute.cs`,
      `Attributes/ConnectorParameterAttribute.cs`, `IConnector.cs`, `ISchemaDiscovery.cs`,
      `IConnectionTest.cs`, `IPermissionTest.cs`, `PostgreSqlConnector.cs`, `TestConnector.cs`,
      `CommonConnectionParameters.cs`, and `TheGrid.Shared/Models/ConnectionProperty.cs` /
      `ConnectorRow.cs` to confirm current signatures before writing (they may have shifted further
      since this proposal was written).

## 2. Rewrite the document

- [x] 2.1 Replace the "Getting started" section's base-class/attribute names
      (`QueryRunnerBase`/`QueryRunnerAttribute`) with `ConnectorBase`/`ConnectorAttribute`.
- [x] 2.2 Replace the invented `MyDatabaseRunner` worked example with one built around
      `PostgreSqlConnector` — quote real excerpts (class declaration with attributes, constructor,
      `GetDataAsync`) directly from `PostgreSqlConnector.cs`, referencing the file path so it can't be
      copy-pasted out of sync silently.
- [x] 2.3 Rewrite the `GetDataAsync` section to document the actual signature —
      `IAsyncEnumerable<ConnectorRow> GetDataAsync(string query, Dictionary<string, object?>?
      queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)` — and
      show the `yield return new ConnectorRow(columns, row)` streaming pattern, not a buffered
      `QueryResult` return. Explain `ConnectorRow`'s `Columns`/`Data` shape and that `Columns` is built
      once (from the first row) and reused for every row in the stream.
- [x] 2.4 Rewrite the `ConnectorParameterAttribute` table to match real properties (`Name`,
      `RenderOrder`, `HelpText` ≤200 chars, `Type` : `ConnectionPropertyType`, `Required`) — remove the
      stale `QueryRunnerParameterType` values table and replace with the real
      `ConnectionPropertyType` values (`SingleLineText`, `MultipleLineText`, `ProtectedText`, `Numeric`,
      `Boolean`).
- [x] 2.5 Add an explicit note (current-behavior, not aspirational) that `ConnectorParameters` dictionary
      keys are the attribute's `Name` string itself — there's no separate machine key yet — and that
      renaming a parameter's `Name` breaks existing stored connections. Reference
      `CommonConnectionParameters` constants as the recommended way to avoid typos in common names.
- [x] 2.6 Rewrite the "Adding database schema discovery support" section around the real
      `ISchemaDiscovery.GetSchemaAsync` signature, using `PostgreSqlConnector.GetSchemaAsync`'s actual
      approach (querying `information_schema`) as the reference rather than the invented example.
- [x] 2.7 Add a section documenting `IConnectionTest.TestConnectionAsync() : Task<bool>` and
      `IPermissionTest.HasWritePermissionAsync() : Task<bool>` as additional opt-in capability
      interfaces, with one real example each from `PostgreSqlConnector`.
- [x] 2.8 Remove any remaining references to `QueryRunner`/`QueryRunnerBase`/`RunQueryAsync`/
      `QueryRunnerAttribute`/`QueryRunnerParameterAttribute`/`QueryRunnerParameterType` anywhere in the
      file — grep the file after editing to confirm zero matches.

## 3. Verify

- [x] 3.1 Grep `docs/Creating-Connectors.md` for `QueryRunner` (case-insensitive) — must return no
      matches.
- [x] 3.2 Manually diff every code excerpt in the doc against its source file
      (`PostgreSqlConnector.cs`) line-by-line to confirm it's a faithful excerpt, not paraphrased or
      stale.
- [x] 3.3 Confirm the doc's attribute/type/method names all exist in the current codebase (spot-check
      via grep for each: `ConnectorAttribute`, `ConnectorParameterAttribute`, `ConnectorBase`,
      `ConnectorRow`, `ConnectionPropertyType`, `ISchemaDiscovery`, `IConnectionTest`, `IPermissionTest`).
