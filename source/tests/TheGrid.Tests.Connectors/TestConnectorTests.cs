// <copyright file="TestConnectorTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Diagnostics;
using TheGrid.Connectors;

namespace TheGrid.Tests.Connectors
{
    /// <summary>
    /// Tests for the <see cref="TestConnector"/> streaming behavior.
    /// </summary>
    public class TestConnectorTests
    {
        /// <summary>
        /// Tests that cancelling mid-enumeration stops further row production, proving the stream isn't pre-buffered.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_CancelMidEnumeration_StopsProducingRows_Test()
        {
            // Arrange
            var connector = new TestConnector(new Dictionary<string, string>
            {
                ["NumberOfRows"] = "1000000",
            });

            using var cts = new CancellationTokenSource();

            var rowsRead = 0;

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await foreach (var row in connector.GetDataAsync("SELECT 1", null, cts.Token).WithCancellation(cts.Token))
                {
                    rowsRead++;

                    if (rowsRead == 5)
                    {
                        cts.Cancel();
                    }
                }
            });

            Assert.Equal(5, rowsRead);
        }

        /// <summary>
        /// Tests that requesting far more rows than are consumed does not cause the connector to eagerly generate the full requested count.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_StopsEarly_DoesNotPreMaterializeRemainingRows_Test()
        {
            // Arrange
            var connector = new TestConnector(new Dictionary<string, string>
            {
                ["NumberOfRows"] = "1000000000",
            });

            var rowsRead = 0;
            var stopwatch = Stopwatch.StartNew();

            // Act
            await foreach (var row in connector.GetDataAsync("SELECT 1", null))
            {
                rowsRead++;

                if (rowsRead == 5)
                {
                    break;
                }
            }

            stopwatch.Stop();

            // Assert
            Assert.Equal(5, rowsRead);

            // If rows were pre-materialized into a list before yielding, this would take far longer than a couple of seconds.
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5), $"Expected early stop to be fast, but it took {stopwatch.Elapsed}.");
        }

        /// <summary>
        /// Tests that <see cref="TestConnector.ThrowExceptionQuery"/> surfaces its exception during enumeration rather than before it begins.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_ThrowExceptionQuery_ThrowsDuringEnumeration_Test()
        {
            // Arrange
            var connector = new TestConnector(new Dictionary<string, string>());

            // Act
            var enumerable = connector.GetDataAsync(TestConnector.ThrowExceptionQuery, null);

            // Assert: obtaining the enumerable does not throw; the exception only surfaces once enumeration starts.
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await foreach (var row in enumerable)
                {
                }
            });
        }
    }
}
