## Why

`QueryExecutor.RefreshQueryResultsAsync` buffers an entire query's results in memory — `PostgreSqlConnector.GetDataAsync` reads every row into a `List<Dictionary<string,object?>>` before returning, and `QueryExecutor` then adds every row to the EF change tracker before a single `SaveChanges`. There is no row cap and no query timeout (the `CancellationToken` is threaded through but nothing ever supplies a deadline). A large or runaway query can exhaust worker memory today, and the app has no way to stop it. This is Phase 0 hardening from `docs/roadmap/ChangeSpecs.md` (P0-2, revised 2026-07-25) — going straight to the streaming target shape now rather than a stopgap, since the connector contract can still be broken freely pre-production.

## What Changes

- **BREAKING**: connectors implement a new row-streaming execution method (`IAsyncEnumerable<QueryResultRow>`-shaped) which becomes `QueryExecutor`'s primary execution path, replacing `GetDataAsync`'s role as the primary path.
- `PostgreSqlConnector` rewritten to `yield` rows from its existing `NpgsqlDataReader` loop instead of buffering into a `List`. Column metadata is still discovered from the first row, preserved from today's `GetColumns` behavior.
- `TestConnector` gets a streaming implementation plus a way to generate a large/configurable row count, so tests can exercise `MaxRows`/truncation behavior without a real database.
- `QueryExecutor.RefreshQueryResultsAsync` enumerates the stream instead of awaiting a fully-buffered result:
  - Stops at a configurable `MaxRows` and marks the execution as truncated.
  - Batches inserts (`SaveChanges` every N rows, clearing the EF change tracker) instead of accumulating every row before one save.
  - Wraps enumeration in a linked `CancellationTokenSource` driven by a configurable timeout; a timeout is recorded as a distinct status rather than a generic error.
- New `Truncated` bool on `QueryExecution`.
- New `TimedOut` value on `QueryExecutionStatus` (today: `None`/`InProgress`/`Complete`/`Error`).
- New execution-limits configuration (`MaxRows`, `Timeout`) sourced from `SystemOptions`. The shape leaves room for a future per-query override but that override is not built in this change.
- EF Core migrations added in both `TheGrid.Postgres` and `TheGrid.Sqlite` for the new column and enum value.

**Explicitly out of scope** (deliberately deferred, do not fold in): connector parameter `Key`/`IsSecret` metadata (P1-1); any Hangfire/Kubernetes worker-scaling, queue-splitting, or self-hosted-worker dispatch architecture — those were explored separately and parked as future work, not inputs to this change's design.

## Capabilities

### New Capabilities
- `connector-streaming`: the connector SDK contract for producing query results as a row stream instead of a fully-buffered result set, including how column metadata is discovered mid-stream.
- `query-execution-limits`: `QueryExecutor`'s enforcement of row caps, query timeouts, truncation reporting, and batched persistence while executing a query.

### Modified Capabilities
_None._ (`connection-secret-management` is unrelated to this change.)

## Impact

- `source/TheGrid.Connectors/IConnector.cs`, `PostgreSqlConnector.cs`, `TestConnector.cs`, `ConnectorBase.cs` — new streaming contract.
- `source/TheGrid.Services/QueryExecutor.cs` — streaming enumeration, batching, timeout, MaxRows enforcement.
- `source/TheGrid.Models/Configuration/SystemOptions.cs` — new execution-limits settings.
- `source/TheGrid.Models/QueryExecution.cs` — new `Truncated` property.
- `source/TheGrid.Shared/Models/QueryExecutionStatus.cs` — new `TimedOut` value.
- `source/TheGrid.Postgres/Migrations/*`, `source/TheGrid.Sqlite/Migrations/*` — new migration in each provider assembly.
- `tests/TheGrid.Tests.Services/QueryExecutorTests.cs`, connector test projects — updated/added coverage for streaming, truncation, and timeout behavior.
