// <copyright file="QueryExecutorTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Models.Configuration;
using TheGrid.Services;
using TheGrid.Services.Hubs;
using TheGrid.Services.Security;
using TheGrid.Tests.Services.Fixtures;
using Xunit.Abstractions;

namespace TheGrid.Tests.Services
{
    /// <summary>
    /// Tests for the <see cref="QueryExecutor"/> class.
    /// </summary>
    public class QueryExecutorTests : IClassFixture<QueryExecutorDatabaseFixture>
    {
        private readonly QueryExecutorDatabaseFixture _fixture;
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryExecutor> _logger;
        private readonly ISecretProtector _secretProtector = new AesGcmSecretProtector(Options.Create(new SecretProtectionOptions
        {
            EncryptionKey = Convert.ToBase64String(new byte[32]),
        }));

        private readonly IOptions<SystemOptions> _defaultSystemOptions = Options.Create(new SystemOptions
        {
            ExecutionLimits = new ExecutionLimits
            {
                MaxRows = 50_000,
                TimeoutSeconds = 120,
                BatchSize = 500,
            },
        });

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutorTests"/> class.
        /// </summary>
        /// <param name="queryExecutorDatabaseFixture">In memory database provider fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public QueryExecutorTests(QueryExecutorDatabaseFixture queryExecutorDatabaseFixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = queryExecutorDatabaseFixture;
            _db = queryExecutorDatabaseFixture.Db;
            _logger = XUnitLogger.CreateLogger<QueryExecutor>(testOutputHelper);
        }

        /// <summary>
        /// Tests the ability to refresh query results.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_Success_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            var execution = new QueryExecution
            {
                JobId = Guid.NewGuid().ToString(),
                QueryId = _fixture.ValidQueryId,
            };

            _db.QueryExecutions.Add(execution);
            await _db.SaveChangesAsync();

            // Act
            var executor = new QueryExecutor(_db, _logger, hubContext, _secretProtector, _defaultSystemOptions);

            await executor.RefreshQueryResultsAsync(execution.Id);

            // Assert
            var results = await _db.QueryResultRows.Where(r => r.QueryExecutionId == execution.Id).ToListAsync();

            Assert.NotEmpty(results);
        }

        /// <summary>
        /// Tests the ability to refresh query results.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_No_Execution_Found_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            long executionId = -1;

            var executor = new QueryExecutor(_db, _logger, hubContext, _secretProtector, _defaultSystemOptions);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await executor.RefreshQueryResultsAsync(executionId));
        }

        /// <summary>
        /// Tests that when executing a query that fails that we get some type of error message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_Fails_Execution_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            var execution = new QueryExecution
            {
                JobId = Guid.NewGuid().ToString(),
                QueryId = _fixture.FailsExecutionQueryId,
            };

            _db.QueryExecutions.Add(execution);
            await _db.SaveChangesAsync();

            // Act
            var executor = new QueryExecutor(_db, _logger, hubContext, _secretProtector, _defaultSystemOptions);

            var exception = await Assert.ThrowsAnyAsync<Exception>(async () => await executor.RefreshQueryResultsAsync(execution.Id));

            // Assert
            var results = await _db.QueryExecutions.Where(e => e.Id == execution.Id).FirstOrDefaultAsync();
            Assert.NotNull(exception);
            _logger.LogInformation("Found exception: {exceptionMessage}", exception.Message);
            Assert.NotNull(results);

            Assert.Equal(TheGrid.Shared.Models.QueryExecutionStatus.Error, results.Status);
        }

        /// <summary>
        /// Tests that a connection's encrypted secret properties are decrypted and merged with its plaintext properties before the connector is constructed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_DecryptsSecretConnectionProperties_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            const int expectedRowCount = 3;

            var connection = new Connection
            {
                Name = "Connection with secret",
                OrganizationId = _fixture.OrganizationId,
                ConnectorId = TheGrid.TestHelpers.Fixtures.OrganizationWithConnection.GetTestConnectorId(),
                SecretProperties = new Dictionary<string, string?>
                {
                    ["NumberOfRows"] = _secretProtector.Protect(expectedRowCount.ToString()),
                },
            };

            _db.Connections.Add(connection);
            await _db.SaveChangesAsync();

            var query = new Query
            {
                Name = "Query using secret connection property",
                Command = "SELECT * FROM TestTable",
                Description = "Test query for secret decryption.",
                ConnectionId = connection.Id,
                Columns =
                [
                    new() { Name = "TextField", Type = QueryResultColumnType.Text },
                    new() { Name = "NumericField", Type = QueryResultColumnType.Integer },
                ],
            };

            _db.Queries.Add(query);
            await _db.SaveChangesAsync();

            var execution = new QueryExecution
            {
                JobId = Guid.NewGuid().ToString(),
                QueryId = query.Id,
            };

            _db.QueryExecutions.Add(execution);
            await _db.SaveChangesAsync();

            var executor = new QueryExecutor(_db, _logger, hubContext, _secretProtector, _defaultSystemOptions);

            // Act
            await executor.RefreshQueryResultsAsync(execution.Id);

            // Assert
            var results = await _db.QueryResultRows.Where(r => r.QueryExecutionId == execution.Id).ToListAsync();

            Assert.Equal(expectedRowCount, results.Count);
        }

        /// <summary>
        /// Tests that a result set larger than the configured <see cref="ExecutionLimits.MaxRows"/> is truncated rather than treated as an error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_MaxRowsExceeded_TruncatesResults_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            const int maxRows = 100;
            var executionId = await CreateExecutionAsync(numberOfRows: 1000);

            var options = Options.Create(new SystemOptions
            {
                ExecutionLimits = new ExecutionLimits { MaxRows = maxRows, TimeoutSeconds = 120, BatchSize = 50 },
            });

            var executor = new QueryExecutor(_db, _logger, hubContext, _secretProtector, options);

            // Act
            await executor.RefreshQueryResultsAsync(executionId);

            // Assert
            var execution = await _db.QueryExecutions.SingleAsync(e => e.Id == executionId);
            var results = await _db.QueryResultRows.Where(r => r.QueryExecutionId == executionId).ToListAsync();

            Assert.Equal(maxRows, results.Count);
            Assert.True(execution.Truncated);
            Assert.Equal(TheGrid.Shared.Models.QueryExecutionStatus.Complete, execution.Status);
        }

        /// <summary>
        /// Tests that a result set at or below the configured <see cref="ExecutionLimits.MaxRows"/> is not marked as truncated.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_WithinMaxRows_NotTruncated_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            const int rowCount = 10;
            var executionId = await CreateExecutionAsync(numberOfRows: rowCount);

            var options = Options.Create(new SystemOptions
            {
                ExecutionLimits = new ExecutionLimits { MaxRows = 100, TimeoutSeconds = 120, BatchSize = 50 },
            });

            var executor = new QueryExecutor(_db, _logger, hubContext, _secretProtector, options);

            // Act
            await executor.RefreshQueryResultsAsync(executionId);

            // Assert
            var execution = await _db.QueryExecutions.SingleAsync(e => e.Id == executionId);
            var results = await _db.QueryResultRows.Where(r => r.QueryExecutionId == executionId).ToListAsync();

            Assert.Equal(rowCount, results.Count);
            Assert.False(execution.Truncated);
        }

        /// <summary>
        /// Tests that an execution exceeding the configured timeout is recorded with a <see cref="TheGrid.Shared.Models.QueryExecutionStatus.TimedOut"/> status.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_TimeoutExceeded_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            var executionId = await CreateExecutionAsync(numberOfRows: 1_000_000);

            var options = Options.Create(new SystemOptions
            {
                ExecutionLimits = new ExecutionLimits { MaxRows = int.MaxValue, TimeoutSeconds = 0, BatchSize = 500 },
            });

            var executor = new QueryExecutor(_db, _logger, hubContext, _secretProtector, options);

            // Act
            await executor.RefreshQueryResultsAsync(executionId);

            // Assert
            var execution = await _db.QueryExecutions.SingleAsync(e => e.Id == executionId);

            Assert.Equal(TheGrid.Shared.Models.QueryExecutionStatus.TimedOut, execution.Status);
        }

        /// <summary>
        /// Tests that a large result set is persisted in multiple batches instead of a single unbounded save.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_BatchesSaveChanges_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            var executionId = await CreateExecutionAsync(numberOfRows: 1000);

            var options = Options.Create(new SystemOptions
            {
                ExecutionLimits = new ExecutionLimits { MaxRows = 100_000, TimeoutSeconds = 60, BatchSize = 100 },
            });

            var executor = new QueryExecutor(_db, _logger, hubContext, _secretProtector, options);

            var saveCount = 0;
            void OnSavingChanges(object? sender, SavingChangesEventArgs e) => saveCount++;
            _db.SavingChanges += OnSavingChanges;

            try
            {
                // Act
                await executor.RefreshQueryResultsAsync(executionId);
            }
            finally
            {
                _db.SavingChanges -= OnSavingChanges;
            }

            // Assert: initial status save + several mid-stream batch saves + final save, not just one.
            Assert.True(saveCount > 2, $"Expected more than 2 SaveChanges calls to prove batching, but got {saveCount}.");
        }

        /// <summary>
        /// Creates a connection, query, and execution record configured to generate the given number of rows via <see cref="TheGrid.Connectors.TestConnector"/>.
        /// </summary>
        /// <param name="numberOfRows">Number of rows the connection's query should generate.</param>
        /// <returns>The ID of the created <see cref="QueryExecution"/>.</returns>
        private async Task<long> CreateExecutionAsync(int numberOfRows)
        {
            var connection = new Connection
            {
                Name = "Connection " + Guid.NewGuid(),
                OrganizationId = _fixture.OrganizationId,
                ConnectorId = TheGrid.TestHelpers.Fixtures.OrganizationWithConnection.GetTestConnectorId(),
                ConnectionProperties = new Dictionary<string, string?>
                {
                    ["NumberOfRows"] = numberOfRows.ToString(),
                },
            };

            _db.Connections.Add(connection);
            await _db.SaveChangesAsync();

            var query = new Query
            {
                Name = "Query " + Guid.NewGuid(),
                Command = "SELECT * FROM TestTable",
                Description = "Test query.",
                ConnectionId = connection.Id,
                Columns =
                [
                    new() { Name = "TextField", Type = QueryResultColumnType.Text },
                    new() { Name = "NumericField", Type = QueryResultColumnType.Integer },
                ],
            };

            _db.Queries.Add(query);
            await _db.SaveChangesAsync();

            var execution = new QueryExecution
            {
                JobId = Guid.NewGuid().ToString(),
                QueryId = query.Id,
            };

            _db.QueryExecutions.Add(execution);
            await _db.SaveChangesAsync();

            return execution.Id;
        }
    }
}
