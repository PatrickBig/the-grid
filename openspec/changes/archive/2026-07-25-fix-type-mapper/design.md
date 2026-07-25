## Context

`TypeExtensions.GetQueryResultColumnTypeForType` (`TheGrid.Connectors/Extensions/TypeExtensions.cs`) is
an `if`/`else if` chain matching a CLR `Type` (after unwrapping `Nullable<T>`) to a
`QueryResultColumnType`. It's called once per column, from the first row of a streamed result set
(`PostgreSqlConnector`/`TestConnector` build `QueryResultColumn` from the reader's field types).

Both `TheGrid.Shared.Models.QueryResultColumnType` and `TheGrid.Models.QueryResultColumnType` are
separately declared enums with identical members today. `QueryExecutor.UpdateColumnDefinitions` converts
between them with Mapster's `.Adapt<Models.QueryResultColumnType>()` — no custom `TypeAdapterConfig`
exists for this enum pair, so Mapster falls back to its default enum conversion, which matches by member
**name** (not by declaration order or underlying int value). Confirmed by reading both enum declarations
side by side: same 7 names, same order, no `[Flags]`, no custom values.

The client (`TheGrid.Client/Shared/Visualizations/Table.razor.cs`, `GetTypeForColumnType`) already
`switch`es on the enum with a `_ => typeof(string)` default arm — it's exhaustiveness-safe for new
members today; they'll render as strings until (if ever) given dedicated handling.

## Goals / Non-Goals

**Goals:**
- One CLR type maps to exactly one `QueryResultColumnType`; no dead/unreachable branches.
- `uint` resolves correctly (to `Long`, since `uint.MaxValue` exceeds `int` range).
- Common previously-uncovered shapes (`Guid`, binary, JSON) get real enum values instead of silently
  collapsing to `Text`.
- Both `QueryResultColumnType` enums (`Shared` and `Models`) stay name-for-name identical so the
  existing unconfigured Mapster `.Adapt<>()` keeps working without adding a custom mapping config.

**Non-Goals:**
- No change to the connector streaming contract or `ConnectorRow` shape.
- No UI work to actually render `Guid`/`Binary`/`Json` columns specially — they render as strings via
  the existing default arm, same as today. That's future work if/when it matters.
- No attempt to detect JSON-typed *string* columns (e.g. Postgres `jsonb` surfacing as `string` in
  .NET) — this only covers CLR types that are structurally JSON (`JsonElement`/`JsonDocument`). Native
  `jsonb`/`json` column detection would require reading Npgsql's provider-specific type info, which is
  out of scope (the Roadmap's §3.4 "native type name" escape hatch is a separate, larger change).

## Decisions

**Rewrite as a `switch` expression keyed on `Type`, not a dictionary lookup.**
A `Dictionary<Type, QueryResultColumnType>` was considered (the roadmap doc suggests it), but a
`switch` expression on the unwrapped `underlyingType` reads just as clearly for ~10 cases, needs no
static dictionary initialization, and keeps the existing method shape (small diff, easy to review).
Either approach fixes the bug; switch wins on simplicity for this size.

**`uint` maps to `Long`, not a new `UnsignedInteger` type.**
`QueryResultColumnType` describes result-set *display* typing, not storage precision. `uint`'s range
(0 to ~4.29B) doesn't fit `int` but fits comfortably in `long`. Adding a distinct unsigned-type enum
member would ripple into the client's `GetTypeForColumnType` switch and the Mapster pair for no display
benefit — `Long` is the correct existing bucket.

**Add `Unknown` as the fallback arm, replacing the current silent `Text` fallback.**
Today any unmapped CLR type (e.g. `float`, `double`, `char`, `sbyte`, `byte`) silently becomes `Text`.
An explicit `Unknown` makes "we don't have a mapping for this" distinguishable from "this genuinely is
text" — useful for future debugging/telemetry. The client's default `_ => typeof(string)` arm still
handles `Unknown` the same as `Text` today (renders as string), so this is non-breaking.

**`Guid`, `byte[]`, JSON (`JsonElement`/`JsonDocument`) get dedicated enum members (`Guid`, `Binary`,
`Json`).**
These are common real-world column shapes (Postgres `uuid`, `bytea`, `json`/`jsonb` when read back as
`JsonDocument`) that today all silently become `Text`, per Roadmap §3.4. Naming them now — even before
the UI does anything special with them — gives future visualization work a real signal to switch on
instead of re-deriving it from Postgres-specific metadata later.

## Risks / Trade-offs

- **[Risk]** `TheGridDbContext` has no `HasConversion` configured for `Models.Column.Type` (verified in
  `OnModelCreating` — every other JSON-shaped column has an explicit converter, this one doesn't), so
  EF Core stores it as its **raw underlying `int`** in the database, not as a string. (The
  `[JsonConverter(typeof(JsonStringEnumConverter))]` attribute only affects System.Text.Json API
  serialization, not EF Core column storage — those are separate concerns.) Inserting a new member
  in the middle of the enum would silently renumber every value after it and corrupt already-stored
  column-type data.
  **[Mitigation]** New members (`Guid`, `Binary`, `Json`, `Unknown`) must be appended at the end of
  both enums, never inserted between existing members. tasks.md calls this out explicitly.
- **[Risk]** Forgetting to add a member to *both* `Shared` and `Models` enums breaks the unconfigured
  Mapster `.Adapt<>()` silently (Mapster would throw or drop the value at runtime, not compile-time,
  since there's no static enum-exhaustiveness check across two separate enum types).
  **[Mitigation]** A unit test asserting the two enums have identical member names (e.g. via
  reflection: `Enum.GetNames(typeof(Shared.QueryResultColumnType))` sequence-equals
  `Enum.GetNames(typeof(Models.QueryResultColumnType))`) added as part of this change, so any future
  drift between the two fails a test instead of failing silently at runtime.

## Migration Plan

No EF migration needed — the column's underlying type (`int`) doesn't change, only which symbolic
names map to which existing/new int values. As long as new members are appended after the existing 7
(not inserted between them), every already-persisted `Column.Type` value keeps meaning exactly what it
meant before.

## Open Questions

None — scope is self-contained and non-breaking.
