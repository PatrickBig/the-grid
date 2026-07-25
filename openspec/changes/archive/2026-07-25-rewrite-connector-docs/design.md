## Context

`docs/Creating-Connectors.md` was written against an earlier version of the SDK (`QueryRunner*`
naming) that was later renamed to `Connector*` and, more recently, changed from a buffered
`GetDataAsync(...)  : Task<QueryResult>` to a streaming `GetDataAsync(...) : IAsyncEnumerable<ConnectorRow>`
(the `streaming-query-execution-limits` change, archived in `openspec/changes/archive/`). The doc was
never updated across either transition. This is a pure documentation change — no design decisions
about system behavior are needed. This design.md exists only because the schema requires it as a
dependency of tasks.md; it records the structural approach for the rewrite itself.

## Goals / Non-Goals

**Goals:**
- Every type, method signature, and attribute usage in the doc compiles against the current code as of
  this change.
- `PostgreSqlConnector` is the running worked example (not an invented class), so future code changes
  to it are the natural trigger to notice the doc needs a re-check.
- Current known traps (display-name-keyed parameters, no secret flag) are documented as *current
  behavior* — not omitted, not described as fixed — since P1-1 hasn't landed yet.

**Non-Goals:**
- No speculative documentation of Phase 1 changes (`Key`/`IsSecret`, `IConnectorFactory`, Abstractions
  split) that haven't happened yet — document only what exists today. Each of those changes should
  update this doc itself when it lands.
- No changes to `docs/architecture/CurrentState.md` or `docs/Roadmap.md` (those already reflect current
  state accurately as of the 2026-07-25 review).

## Decisions

**Structure the doc as: attribute/base-class walkthrough → worked example referencing
`PostgreSqlConnector` directly → capability interfaces section**, mirroring the doc's existing overall
shape (which is reasonable) rather than reinventing the structure. Only the content within each section
needs correcting, not the outline.

**Reference `PostgreSqlConnector` by pointing at it, not by pasting a full inline copy that could
drift.** Prior version's approach (a fully inline invented `MyDatabaseRunner` class) is exactly how it
went stale — an inline example has no compiler checking it. Quote short, real excerpts from
`PostgreSqlConnector.cs`/`TestConnector.cs` where a code sample is genuinely useful, but keep the doc
pointing the reader at the real file for the complete picture.

## Risks / Trade-offs

- **[Risk]** The doc drifts again the next time the connector contract changes (P1-1/P1-2/P1-3 will all
  touch these signatures).
  **[Mitigation]** The `connector-authoring-guide` spec created by this change gives each future
  contract-changing OpenSpec change a concrete existing requirement to write a MODIFIED delta against,
  rather than the doc being untracked prose no change proposal is obligated to touch.

## Migration Plan

None — documentation-only, no code or data affected.

## Open Questions

None.
