// <copyright file="TestConnector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TheGrid.Shared.Models;

namespace TheGrid.Connectors
{
    /// <summary>
    /// This is a simulation connector used for tests only. There is a special exclusion to prevent this connector from being made available.
    /// </summary>
    /// <param name="connectorParameters">Parameters used to connect to the test database.</param>
    [Connector("Test Connector", EditorLanguage = EditorLanguage.Sql)]
    [ConnectorParameter(CommonConnectionParameters.ConnectionString, ConnectionPropertyType.SingleLineText)]
    [ConnectorParameter("NumberOfRows", ConnectionPropertyType.Numeric)]
    [ExcludeFromCodeCoverage]
    public class TestConnector(Dictionary<string, string> connectorParameters) : ConnectorBase(connectorParameters)
    {
        /// <summary>
        /// Query to use to force no columns to be returned in the result, which can generate errors.
        /// </summary>
        public const string ThrowExceptionQuery = "THROW AN EXCEPTION";

        private readonly Random _random = new();

        /// <inheritdoc/>
        public override async IAsyncEnumerable<ConnectorRow> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (query == ThrowExceptionQuery)
            {
                throw new InvalidOperationException("This query was expected to fail for tests.");
            }

            var columns = new Dictionary<string, QueryResultColumn>
            {
                { "TextField", new QueryResultColumn { Type = QueryResultColumnType.Text } },
                { "NumericField", new QueryResultColumn { Type = QueryResultColumnType.Integer } },
            };

            // Generate some rows, one at a time, without pre-materializing the full set.
            var numberOfRowsToGenerate = NumberOfRowsToGenerate();

            for (int i = 0; i < numberOfRowsToGenerate; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return new ConnectorRow(columns, GenerateRow());

                // Yield control so an await foreach consumer can observe cancellation/early-stop between rows.
                await Task.Yield();
            }
        }

        private int NumberOfRowsToGenerate()
        {
            if (ConnectorParameters.TryGetValue("NumberOfRows", out var rowValue) && int.TryParse(rowValue, out int numberOfRows))
            {
                return numberOfRows;
            }

            return 10;
        }

        private Dictionary<string, object?> GenerateRow()
        {
            return new Dictionary<string, object?>
            {
                {
                    "TextField", "Random text " + _random.Next().ToString()
                },
                {
                    "NumericField", _random.Next()
                },
            };
        }
    }
}
