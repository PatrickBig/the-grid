// <copyright file="QueryDataConverterTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.Json;
using TheGrid.Shared.Utilities;
using Xunit.Abstractions;

namespace TheGrid.Shared.Unit.Tests
{
    /// <summary>
    /// Tests for the <see cref="QueryDataConverter"/> class.
    /// </summary>
    public class QueryDataConverterTests
    {
        private readonly ITestOutputHelper _output;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            Converters =
            {
                new QueryDataConverter(),
            },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDataConverterTests"/> class.
        /// </summary>
        /// <param name="output">Test output helper.</param>
        public QueryDataConverterTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Tests the ability to deserialize test data.
        /// </summary>
        [Fact]
        public void TestQueryDataDeserializer_Test()
        {
            // Arrange
            var jsonData = File.ReadAllText(Path.Join("TestData", "test-query-data-success-data.json"));

            // Act
            var obj = JsonSerializer.Deserialize<TestQueryData>(jsonData, _serializerOptions);

            // Assert
            Assert.NotNull(obj);

            foreach (var row in obj.Items)
            {
                foreach (var kv in row)
                {
                    if (kv.Value != null)
                    {
                        _output.WriteLine("Column: " + kv.Key + ", Value: " + kv.Value + ", Type: " + kv.Value.GetType().ToString());
                    }
                    else
                    {
                        _output.WriteLine("Column: " + kv.Key + ", Value: " + kv.Value + ", Type: unknown");
                    }
                }
            }
        }

        private class TestQueryData
        {
            public List<Dictionary<string, object?>> Items { get; set; } = new();
        }
    }
}