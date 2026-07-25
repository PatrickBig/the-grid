## 1. Shared connector contract

- [x] 1.1 Add `ConnectorRow` record to `TheGrid.Shared.Models` (`Columns: IReadOnlyDictionary<string, QueryResultColumn>`, `Data: IReadOnlyDictionary<string, object?>`)
- [x] 1.2 Change `IConnector.GetDataAsync` to return `IAsyncEnumerable<ConnectorRow>` (use `[EnumeratorCancellation]` on the token parameter)
- [x] 1.3 Update `ConnectorBase` if it has any shared members touching the old `QueryResult`-returning signature
- [x] 1.4 Decide fate of `QueryResult`/`QueryResult.StandardOutput` (`TheGrid.Shared.Models`) — confirm nothing else references the buffered envelope before removing or leaving it unused; do not carry it forward as a second return shape

## 2. Connector implementations

- [x] 2.1 Rewrite `PostgreSqlConnector.GetDataAsync` as `async IAsyncEnumerable<ConnectorRow>`, building the `Columns` dictionary once from the reader on the first row (reuse existing `GetColumns` logic) and yielding a `ConnectorRow` per row
- [x] 2.2 Verify the existing `NpgsqlDataReader` cancellation behavior is preserved through the `yield return` loop (token still passed to `ReadAsync`)
- [x] 2.3 Add a streaming implementation to `TestConnector` with a configurable row count
- [x] 2.4 Ensure `TestConnector`'s row generation does not pre-materialize all rows before yielding (must support early-stop without generating the full requested count)
- [x] 2.5 Confirm `TestConnector.ThrowExceptionQuery` behavior still works when converted to a streaming method (exception should surface during enumeration, not before)

## 3. Execution limits configuration

- [x] 3.1 Add execution limits (`MaxRows`, timeout) and a batch size setting to `SystemOptions` (or a nested `ExecutionLimits` type referenced from it — see design.md Open Questions)
- [x] 3.2 Choose and document sensible defaults for `MaxRows`, timeout, and batch size
- [x] 3.3 Wire the new settings through configuration binding (`appsettings.json` / `appsettings.Development.json`) with the chosen defaults

## 4. Model changes

- [x] 4.1 Add `Truncated` (bool, default `false`) to `TheGrid.Models.QueryExecution`
- [x] 4.2 Add `TimedOut` value to `TheGrid.Shared.Models.QueryExecutionStatus`
- [x] 4.3 Add EF Core migration in `TheGrid.Postgres` for the new column/enum usage
- [x] 4.4 Add EF Core migration in `TheGrid.Sqlite` for the new column/enum usage
- [x] 4.5 Verify `TheGridDbContextModelSnapshot.cs` is regenerated correctly for both providers

## 5. QueryExecutor rewrite

- [x] 5.1 Replace the buffered `results.Rows` loop in `RefreshQueryResultsAsync` with `await foreach` enumeration of `connector.GetDataAsync(...)`
- [x] 5.2 Create a linked `CancellationTokenSource` with `CancelAfter(timeout)` before starting enumeration
- [x] 5.3 Stop enumeration and set `Truncated = true` once `MaxRows` is reached (without erroring)
- [x] 5.4 Batch `SaveChanges` every N rows (configured batch size) and call `ChangeTracker.Clear()` after each batch
- [x] 5.5 Distinguish a timeout-triggered cancellation from a caller-triggered cancellation (per design.md Decision 4 / Risk) and set `QueryExecutionStatus.TimedOut` only for the former
- [x] 5.6 Update `UpdateColumnDefinitions` call site to read `Columns` from the first `ConnectorRow` instead of `results.Columns`
- [x] 5.7 Ensure the `finally` block's `SaveChangesAsync` still correctly persists the final partial batch and status on both normal completion and truncation/timeout/error paths

## 6. Tests

- [x] 6.1 Update `QueryExecutorTests.cs` for the new streaming call shape
- [x] 6.2 Add a test: result set larger than `MaxRows` → execution truncated, status `Complete`, row count capped at `MaxRows`
- [x] 6.3 Add a test: result set at/below `MaxRows` → `Truncated` is `false`
- [x] 6.4 Add a test: execution exceeding the configured timeout → status `TimedOut`
- [x] 6.5 Add a test: large result set → `SaveChanges` is called multiple times (batching actually occurs), not once
- [x] 6.6 Add a connector-level test: cancelling mid-enumeration stops further row production (proves the stream isn't secretly pre-buffered)
- [x] 6.7 Add a connector-level test: `TestConnector` requested for more rows than `MaxRows`, consumer stops early, assert the connector did not generate rows beyond what was consumed

## 7. Docs

- [x] 7.1 Update `docs/architecture/CurrentState.md`'s connector SDK contract table to reflect the new `IConnector.GetDataAsync` streaming signature and the removal/retirement of the buffered `QueryResult` envelope
- [x] 7.2 Update `docs/roadmap/ChangeSpecs.md` to mark this change (revised P0-2 / superseded P0-3 / pulled-forward P1-4) as done once implemented
