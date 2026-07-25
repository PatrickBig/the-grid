## ADDED Requirements

### Requirement: Query execution enforces a maximum row count
The system SHALL stop reading query results after a configured maximum number of rows (`MaxRows`) has been received, and SHALL mark the execution as truncated rather than treating the cutoff as an error.

#### Scenario: Result set exceeds MaxRows
- **WHEN** a query's underlying result set contains more rows than the configured `MaxRows`
- **THEN** the executor stops enumerating further rows once `MaxRows` has been reached
- **AND** the resulting `QueryExecution` has `Truncated` set to `true`
- **AND** the execution status is `Complete`, not `Error`

#### Scenario: Result set within MaxRows
- **WHEN** a query's result set contains fewer rows than the configured `MaxRows`
- **THEN** all rows are persisted
- **AND** the resulting `QueryExecution` has `Truncated` set to `false`

### Requirement: Query execution enforces a timeout
The system SHALL cancel an in-progress query execution once a configured timeout elapses, and SHALL record a distinct status indicating the execution timed out rather than a generic error.

#### Scenario: Query exceeds the configured timeout
- **WHEN** a query has not completed producing results after the configured timeout has elapsed
- **THEN** execution is cancelled
- **AND** the resulting `QueryExecution` status is `TimedOut`
- **AND** partial results already persisted before the timeout are not discarded

#### Scenario: Query completes within the timeout
- **WHEN** a query completes before the configured timeout elapses
- **THEN** the resulting `QueryExecution` status is `Complete` (or `Error` if the query itself failed), unaffected by the timeout mechanism

### Requirement: Result rows are persisted in batches
The system SHALL persist query result rows in bounded batches during execution rather than accumulating the entire result set in memory before a single save.

#### Scenario: Large result set is saved incrementally
- **WHEN** a query produces a result set larger than one batch
- **THEN** rows are committed to the database in multiple batches as they are received
- **AND** the number of rows held in memory awaiting persistence at any one time is bounded, independent of total result set size

### Requirement: Execution limits are configurable
`MaxRows` and the query timeout SHALL be sourced from system configuration and applied to every query execution.

#### Scenario: Administrator configures execution limits
- **WHEN** an administrator sets `MaxRows` and timeout values in system configuration
- **THEN** subsequent query executions use those values when enforcing row limits and timeouts

#### Scenario: No per-query override exists yet
- **WHEN** a query is executed
- **THEN** the system-wide configured limits apply uniformly
- **AND** no mechanism exists in this change for an individual query to override those limits
