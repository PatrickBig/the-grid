## Why

`TypeExtensions.GetQueryResultColumnTypeForType` (`TheGrid.Connectors/Extensions/TypeExtensions.cs`)
has a duplicate `long` branch that makes the `uint` check after it dead code — `uint` values silently
resolve to `Text` instead of a numeric type. The mapper also has no support for `Guid`, binary
(`byte[]`), or JSON-shaped values, so all of those collapse to `Text` today, losing type fidelity that
the query results grid and any future type-aware rendering would need.

## What Changes

- Rewrite `GetQueryResultColumnTypeForType` as an explicit, unambiguous type map (dictionary or
  switch expression) with one CLR type resolving to exactly one `QueryResultColumnType`.
- Fix resolution so `uint` maps to `Long` (per its actual numeric range) instead of falling through to
  a dead branch.
- Add mappings for `Guid`, `byte[]` (binary), and JSON-shaped values (`System.Text.Json.JsonElement` /
  `JsonDocument`).
- Extend `QueryResultColumnType` (`TheGrid.Shared/Models/QueryResultColumn.cs`) with `Guid`, `Binary`,
  `Json`, and `Unknown` (fallback for an unmapped CLR type, replacing the current silent `Text`
  fallback).
- Add unit tests covering every mapped CLR type, including the previously-buggy `uint`/`long` cases.
- Verify the Mapster mapping in `QueryExecutor.UpdateColumnDefinitions` (between the `Shared` and
  `Models` column-type enums) stays in sync with the new enum members.

No breaking changes to the connector contract — this only affects internal type-mapping logic and
adds new enum values.

## Capabilities

### New Capabilities
- `query-result-type-mapping`: defines the contract for mapping a CLR `Type` to a `QueryResultColumnType`
  used to describe query result columns, including the full set of supported types and the fallback
  behavior for unmapped types.

### Modified Capabilities
(none — this is new spec coverage for existing-but-previously-unspecified behavior)

## Impact

- `TheGrid.Connectors/Extensions/TypeExtensions.cs` — rewritten mapper.
- `TheGrid.Shared/Models/QueryResultColumn.cs` — expanded `QueryResultColumnType` enum.
- `TheGrid.Services/QueryExecutor.cs` — `UpdateColumnDefinitions`'s Mapster mapping, if it needs new
  arms for the added enum values.
- `TheGrid.Tests.Connectors` — new/expanded unit tests.
- Any code that already assumes only the original 7 `QueryResultColumnType` values exist (e.g. client
  rendering that switches on the enum) should be checked for exhaustiveness, though none is expected to
  break since new values only get hit for previously-`Text`-collapsed CLR types.
