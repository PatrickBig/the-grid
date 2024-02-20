// <copyright file="TestConnector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

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
        private readonly Random _random = new();

        /// <inheritdoc/>
        public override Task<QueryResult> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, CancellationToken cancellationToken = default)
        {
            var results = new QueryResult
            {
                Columns =
                {
                    { "TextField", new QueryResultColumn { Type = QueryResultColumnType.Text } },
                    { "NumericField", new QueryResultColumn { Type = QueryResultColumnType.Integer } },
                },
            };

            // Generate some rows
            var numberOfRowsToGenerate = NumberOfRowsToGenerate();

            for (int i = 0; i < numberOfRowsToGenerate; i++)
            {
                results.Rows.Add(GenerateRow());
            }

            return Task.FromResult(results);
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
