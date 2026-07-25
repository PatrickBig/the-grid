## Context

`QueryExecutor.RefreshQueryResultsAsync` (`source/TheGrid.Services/QueryExecutor.cs`) currently calls `connector.GetDataAsync(query.Command, null, ct)`, which returns a fully-buffered `QueryResult` (`Dictionary<string,QueryResultColumn> Columns`, `List<Dictionary<string,object?>> Rows`, `List<string> StandardOutput`). `PostgreSqlConnector.GetDataAsync` reads every row from its `NpgsqlDataReader` into a `List` before returning; `QueryExecutor` then adds every row to the EF change tracker and calls `SaveChanges` once at the end. There is no row cap and no timeout.

This is a solo, pre-production project — the connector contract can be broken freely. This design goes straight to a streaming execution model rather than staging a "cheap now, real later" fix, per the decision recorded in `docs/roadmap/ChangeSpecs.md` (P0-2, revised 2026-07-25).

Note: `QueryResult.StandardOutput` and the matching `QueryExecution.StandardOutput` column exist but are never populated by any current connector or by `QueryExecutor` — this is dead capacity today, not a behavior this change needs to preserve. It's left in place on `QueryExecution` (no migration needed for it) but is out of scope for the streaming redesign.

## Goals / Non-Goals

**Goals:**
- Make `PostgreSqlConnector` (and any future connector) stream rows to `QueryExecutor` instead of buffering the full result set in memory.
- Enforce a configurable `MaxRows` cap during execution, marking truncated executions rather than erroring.
- Enforce a configurable timeout on the whole execution, recorded as a distinct `TimedOut` status.
- Persist rows in bounded batches instead of one unbounded `SaveChanges`.
- Keep `TestConnector` and existing tests exercising these paths without a real database.

**Non-Goals:**
- Per-query override of `MaxRows`/timeout (data model should not preclude it later, but no UI/API is built now).
- Connector parameter `Key`/`IsSecret` metadata (P1-1) — separate, undecided.
- Any change to how jobs are scheduled, queued, or scaled (Hangfire, Kubernetes, KEDA) — explicitly parked as a separate future exploration.
- Preserving `GetDataAsync`'s buffered signature as a fallback/compat path for non-streaming connectors — there are only two connectors today (`PostgreSqlConnector`, `TestConnector`) and both are updated in this change, so no dual-path complexity is needed.

## Decisions

### 1. Replace `GetDataAsync`'s buffered contract outright; no parallel streaming interface
The roadmap originally proposed `IStreamingConnector` as an **optional** capability interface layered on top of a `GetDataAsync` default. This change instead replaces `IConnector`'s single execution method's return shape with a streaming one — there's no reason to carry two code paths in a two-connector, pre-production codebase. `TestConnector` and `PostgreSqlConnector` both implement the new shape directly.

```csharp
public interface IConnector
{
    IAsyncEnumerable<ConnectorRow> GetDataAsync(
        string query,
        Dictionary<string, object?>? queryParameters,
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
}
```

**Alternative considered:** keep `GetDataAsync` buffered and add `StreamDataAsync` as an opt-in interface (the original P1-4 shape). Rejected because it reintroduces exactly the dual-path complexity this change is trying to avoid, for no present benefit — every connector in the codebase is being touched anyway.

### 2. New `ConnectorRow` type carries column metadata alongside each row, instead of a side-channel property
Naming note: `TheGrid.Models.QueryResultRow` already exists as the **persisted EF entity** for a saved result row. Reusing that name for the connector-side streaming DTO would be confusing, so the new shared type is named `ConnectorRow` (`TheGrid.Shared.Models`):

```csharp
public sealed record ConnectorRow(
    IReadOnlyDictionary<string, QueryResultColumn> Columns,
    IReadOnlyDictionary<string, object?> Data);
```

The connector builds the `Columns` dictionary once (as `GetColumns` does today, from the reader's field metadata on the first row) and attaches the same dictionary reference to every yielded `ConnectorRow` — cheap (a reference copy, not a deep copy) and avoids a stateful "read this property after the first MoveNext" pattern that would be easy to misuse.

**Alternative considered:** expose `Columns` as a property on `IConnector` populated as a side effect of enumeration (mirroring `DbDataReader`'s schema-after-first-read semantics). Rejected as a less discoverable contract — a consumer could read the property before enumerating and get nothing, or read it mid-enumeration and get a half-populated result. Attaching columns to every row removes that footgun at a negligible memory cost.

### 3. `QueryExecutor` owns MaxRows/timeout/batching; connectors stay dumb
Connectors do not know about `MaxRows` or timeouts. `PostgreSqlConnector` streams every row its query produces; `QueryExecutor` is the only place that decides when to stop consuming the stream (row cap) or cancel it (timeout). This keeps the row-limiting policy in one place instead of duplicating it per connector, and matches `TestConnector`'s use case: a test can ask `TestConnector` for far more rows than `MaxRows`, and `QueryExecutor` stopping enumeration early is what proves the cap actually short-circuits the stream rather than draining it.

```csharp
await foreach (var row in connector.GetDataAsync(query.Command, null, linkedCts.Token)
    .WithCancellation(linkedCts.Token))
{
    if (rowCount >= executionLimits.MaxRows)
    {
        queryExecution.Truncated = true;
        break;
    }

    _db.QueryResultRows.Add(new QueryResultRow { QueryExecutionId = queryExecutionId, Data = row.Data.ToDictionary() });
    rowCount++;

    if (rowCount % batchSize == 0)
    {
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
    }
}
```

Column definitions (`UpdateColumnDefinitions`) are updated from the first row's `Columns` the same way they're updated from `results.Columns` today.

### 4. Timeout via a linked `CancellationTokenSource`, not a connector-level concern
`QueryExecutor` creates `CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)` and calls `CancelAfter(timeout)` before starting enumeration. Because the `CancellationToken` is already threaded into `NpgsqlDataReader.ReadAsync` today, this requires no connector-side changes to actually stop a running query read — only `QueryExecutor` needs to distinguish "cancelled by the linked timeout source" from "cancelled by the caller" to decide whether to record `TimedOut` vs propagate a plain cancellation.

### 5. Batch size and execution limits are static configuration, not per-query, for now
`ExecutionLimits` (`MaxRows`, `Timeout`) and a batch size are added to `SystemOptions`. A future per-query override is plausible (goals note it should not be precluded) but is not built now — this keeps the change scoped to the OOM/timeout problem instead of also building an override UI/API surface.

### 6. New `Truncated` bool and `TimedOut` status require a migration in both providers
`QueryExecution.Truncated` (bool, default `false`) and `QueryExecutionStatus.TimedOut` (new enum value) both need an EF Core migration added separately to `TheGrid.Postgres` and `TheGrid.Sqlite`, per this repo's standing rule that any schema change is a migration in each provider assembly (`docs/AddingMigrations.md`).

## Risks / Trade-offs

- **[Risk]** `IAsyncEnumerable` + `[EnumeratorCancellation]` semantics are easy to get subtly wrong (e.g. forgetting `.WithCancellation`, or a connector implementing the interface with `async IAsyncEnumerable<T>` but not honoring cancellation inside its own loop) → **Mitigation:** `PostgreSqlConnector`'s existing `while (await reader.ReadAsync(cancellationToken))` loop already takes a token; converting it to `yield return` with the same token preserves the existing cancellation behavior. Add a test that cancels mid-stream and asserts no further rows are produced.
- **[Risk]** Attaching a shared `Columns` dictionary reference to every `ConnectorRow` means a consumer that mutates it would corrupt every row's view → **Mitigation:** type it as `IReadOnlyDictionary<string, QueryResultColumn>`; connectors always construct a fresh dictionary internally, never mutate after the first row.
- **[Risk]** Batching `SaveChanges` + `ChangeTracker.Clear()` mid-execution means a failure partway through leaves partial results committed, whereas today's single-`SaveChanges` failure leaves nothing committed → **Mitigation:** this is an intentional trade-off (bounded memory > all-or-nothing atomicity for large result sets) and is consistent with the `Truncated` requirement's spirit — partial results are useful, not something to hide. Document this behavior change explicitly since it's a semantic shift, not just a performance one.
- **[Risk]** A timeout firing mid-batch needs to distinguish "the linked timeout source fired" from "the caller's own token was cancelled" (e.g. Hangfire shutting down the job) to set `TimedOut` correctly instead of misreporting a shutdown as a timeout → **Mitigation:** check `linkedCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested` before assigning `TimedOut`.

## Migration Plan

1. Add `ConnectorRow` to `TheGrid.Shared.Models`; change `IConnector.GetDataAsync`'s signature.
2. Rewrite `PostgreSqlConnector.GetDataAsync` as `async IAsyncEnumerable<ConnectorRow>` using `yield return`.
3. Add a streaming implementation + configurable row-count generation to `TestConnector`.
4. Add `Truncated` to `QueryExecution`, `TimedOut` to `QueryExecutionStatus`; add `ExecutionLimits`-style settings + batch size to `SystemOptions`.
5. Rewrite `QueryExecutor.RefreshQueryResultsAsync`'s consumption loop per Decision 3/4.
6. Add EF Core migrations in `TheGrid.Postgres` and `TheGrid.Sqlite`.
7. Update/add tests: `QueryExecutorTests.cs` (MaxRows truncation, timeout → `TimedOut`, batched saves), connector tests (streaming yields expected rows, cancellation stops enumeration).

No rollback complexity beyond a standard EF migration revert — no data backfill is involved (`Truncated` defaults to `false` for existing rows, no existing `TimedOut` rows to reconcile).

## Open Questions

- Should `ExecutionLimits`/batch size be simple flat properties on `SystemOptions`, or a nested `ExecutionLimits` sub-object? Leaning nested (matches the roadmap's original naming) but not load-bearing either way — decide during implementation.
- Exact default values for `MaxRows`, timeout, and batch size are not yet chosen — needs a reasonable default (e.g. `MaxRows` in the tens of thousands, timeout in tens of seconds to a few minutes, batch size ~500) picked during implementation rather than in this design.
