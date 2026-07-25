## ADDED Requirements

### Requirement: CLR type to QueryResultColumnType mapping is unambiguous
Given a .NET `Type` produced by a connector's result set (after unwrapping `Nullable<T>`), the system
SHALL map it to exactly one `QueryResultColumnType` value via a single, unambiguous set of type
comparisons with no unreachable branches.

#### Scenario: uint maps to Long
- **WHEN** a column's underlying CLR type is `uint`
- **THEN** `GetQueryResultColumnTypeForType` returns `QueryResultColumnType.Long`

#### Scenario: long maps to Long
- **WHEN** a column's underlying CLR type is `long`
- **THEN** `GetQueryResultColumnTypeForType` returns `QueryResultColumnType.Long`

#### Scenario: short, ushort, and int map to Integer
- **WHEN** a column's underlying CLR type is `short`, `ushort`, or `int`
- **THEN** `GetQueryResultColumnTypeForType` returns `QueryResultColumnType.Integer`

### Requirement: Guid, binary, and JSON column types are recognized
The system SHALL map `Guid`, `byte[]`, and JSON-shaped CLR types (`System.Text.Json.JsonElement`,
`System.Text.Json.JsonDocument`) to dedicated `QueryResultColumnType` values instead of collapsing them
to `Text`.

#### Scenario: Guid maps to Guid
- **WHEN** a column's underlying CLR type is `Guid`
- **THEN** `GetQueryResultColumnTypeForType` returns `QueryResultColumnType.Guid`

#### Scenario: byte array maps to Binary
- **WHEN** a column's underlying CLR type is `byte[]`
- **THEN** `GetQueryResultColumnTypeForType` returns `QueryResultColumnType.Binary`

#### Scenario: JsonElement or JsonDocument maps to Json
- **WHEN** a column's underlying CLR type is `System.Text.Json.JsonElement` or
  `System.Text.Json.JsonDocument`
- **THEN** `GetQueryResultColumnTypeForType` returns `QueryResultColumnType.Json`

### Requirement: Unmapped CLR types resolve to Unknown, not Text
The system SHALL return `QueryResultColumnType.Unknown` for any CLR type without an explicit mapping,
rather than silently defaulting to `QueryResultColumnType.Text`.

#### Scenario: An unmapped numeric type resolves to Unknown
- **WHEN** a column's underlying CLR type is `float`, `double`, `char`, `sbyte`, or `byte`
- **THEN** `GetQueryResultColumnTypeForType` returns `QueryResultColumnType.Unknown`

### Requirement: QueryResultColumnType enum members stay identical across Shared and Models
`TheGrid.Shared.Models.QueryResultColumnType` and `TheGrid.Models.QueryResultColumnType` SHALL declare
the same set of member names, in the same order, so that Mapster's default (unconfigured) enum
adaptation between them remains correct.

#### Scenario: Enum member names match between the two declarations
- **WHEN** the set of member names from `TheGrid.Shared.Models.QueryResultColumnType` is compared to
  the set of member names from `TheGrid.Models.QueryResultColumnType`
- **THEN** the two sets are identical

### Requirement: New enum members are appended, not inserted
Because `TheGrid.Models.Column.Type` is persisted by EF Core as its raw underlying `int` (no
`HasConversion` is configured for it in `TheGridDbContext`), new `QueryResultColumnType` members SHALL
be added after all existing members in both enum declarations, preserving the existing members'
underlying integer values.

#### Scenario: Existing member ordinal values are unchanged
- **WHEN** the enum is extended with `Guid`, `Binary`, `Json`, and `Unknown`
- **THEN** `Text`, `Boolean`, `Integer`, `Long`, `Decimal`, `DateTime`, and `Time` keep the same
  underlying integer values they had before this change
