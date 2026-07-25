# connector-streaming Specification

## Purpose

TBD - created by syncing change streaming-query-execution-limits. Update Purpose after archive.

## Requirements

### Requirement: Connectors stream query results row-by-row
A connector SHALL expose a streaming execution method that produces query result rows one at a time as they become available from the underlying data source, instead of returning a fully-buffered result set. This streaming method SHALL be the primary path `QueryExecutor` uses to run a query.

#### Scenario: Rows become available incrementally
- **WHEN** a query is executed against a connector that implements streaming
- **THEN** the caller can begin consuming result rows before the underlying data source has produced its last row
- **AND** the connector does not hold the entire result set in memory at once

#### Scenario: Small result set streams to completion
- **WHEN** a query returns a small number of rows
- **THEN** the stream yields every row and completes normally, with no behavior difference visible to the caller compared to today's fully-buffered execution

### Requirement: Column metadata is discovered from the first streamed row
Column name and type metadata SHALL be determined from the first row produced by the stream, matching the current behavior where column metadata is captured on the first row read from the data reader.

#### Scenario: Column metadata available after first row
- **WHEN** the stream yields its first row
- **THEN** the consumer has access to the full set of column names and their mapped types
- **AND** this metadata does not change for subsequent rows in the same execution

#### Scenario: Empty result set
- **WHEN** a query produces zero rows
- **THEN** the stream completes without yielding any rows
- **AND** column metadata reflects the query's declared result shape where the connector is able to provide it, or is empty if it cannot

### Requirement: Test connector supports configurable row volume
`TestConnector` SHALL support generating a configurable, arbitrarily large number of rows via its streaming method, so tests can exercise row-limiting and truncation behavior without a real external data source.

#### Scenario: Requesting a large row count
- **WHEN** a test configures `TestConnector` to produce a row count larger than a configured execution limit
- **THEN** the connector streams rows up to the requested count without pre-buffering them all in memory
- **AND** a consumer stopping enumeration early (e.g. after reaching a row cap) does not cause the connector to generate the remaining rows
