## 1. Expand the shared enums (append-only)

- [x] 1.1 Add `Guid`, `Binary`, `Json`, `Unknown` to `TheGrid.Shared.Models.QueryResultColumnType`
      (`TheGrid.Shared/Models/QueryResultColumn.cs`), appended after `Time` — do not insert between
      existing members.
- [x] 1.2 Add the same four members, in the same order, to `TheGrid.Models.QueryResultColumnType`
      (`TheGrid.Models/Column.cs`).
- [x] 1.3 Add XML doc `<summary>` comments for each new member (matches existing style in both files).

## 2. Rewrite the type mapper

- [x] 2.1 Rewrite `GetQueryResultColumnTypeForType` (`TheGrid.Connectors/Extensions/TypeExtensions.cs`)
      as a `switch` expression on `underlyingType` (after the existing `Nullable.GetUnderlyingType`
      unwrap).
- [x] 2.2 Fix `uint` to resolve to `Long` (remove the dead second `long` arm entirely).
- [x] 2.3 Add arms for `Guid` → `Guid`, `byte[]` → `Binary`,
      `System.Text.Json.JsonElement`/`System.Text.Json.JsonDocument` → `Json`.
- [x] 2.4 Change the fallback arm from `QueryResultColumnType.Text` to `QueryResultColumnType.Unknown`.

## 3. Check the Mapster mapping stays in sync

- [x] 3.1 Confirm `QueryExecutor.UpdateColumnDefinitions`'s `.Adapt<Models.QueryResultColumnType>()`
      calls need no code change (no `TypeAdapterConfig` currently registered for this enum pair — default
      name-based conversion should keep working since both enums now have identical member names).
- [x] 3.2 If a custom `TypeAdapterConfig` is ever found for this enum pair (not expected per design.md's
      Context section), update it to cover the new members too.

## 4. Tests

- [x] 4.1 Add/update unit tests in `TheGrid.Tests.Connectors` covering every mapped CLR type: `short`,
      `ushort`, `int` → `Integer`; `uint`, `long` → `Long`; `decimal` → `Decimal`; `TimeSpan` → `Time`;
      `DateTime` → `DateTime`; `bool` → `Boolean`; `string` → `Text`; `Guid` → `Guid`; `byte[]` →
      `Binary`; `JsonElement`/`JsonDocument` → `Json`.
- [x] 4.2 Add a test for an unmapped type (e.g. `double` or `float`) → `Unknown`.
- [x] 4.3 Add a test for `Nullable<T>` unwrapping still working post-rewrite (e.g. `int?` → `Integer`).
- [x] 4.4 Add a test asserting `Enum.GetNames(typeof(TheGrid.Shared.Models.QueryResultColumnType))`
      sequence-equals `Enum.GetNames(typeof(TheGrid.Models.QueryResultColumnType))`, per design.md's
      Mapster-drift mitigation.
- [x] 4.5 Add a test asserting the underlying `int` values of the original 7 members
      (`Text`=0 … `Time`=6) are unchanged after the enum extension, per design.md's append-only
      requirement.

## 5. Verification

- [x] 5.1 Run `dotnet build TheGrid.sln` — confirm no warnings about unreachable code remain (the
      original duplicate-`long`-branch bug would have produced a compiler warning once visible; confirm
      it's gone).
- [x] 5.2 Run `dotnet test tests/TheGrid.Tests.Connectors/TheGrid.Tests.Connectors.csproj` — all green.
- [x] 5.3 Run `dotnet test tests/TheGrid.Tests.Services/TheGrid.Tests.Services.csproj` — all green
      (covers `QueryExecutor`/Mapster path).
- [x] 5.4 Spot-check `TheGrid.Client/Shared/Visualizations/Table.razor.cs`'s `GetTypeForColumnType` —
      confirm its `_ => typeof(string)` default arm still compiles and handles the new enum members
      without modification (per design.md's Non-Goals).
