using System.Text.Json;
using System.Text.Json.Serialization;
using TheGrid.Shared.Utilities;
using Xunit.Abstractions;

namespace TheGrid.Shared.Unit.Tests
{
    public class QueryDataConverterTests
    {
        private readonly Random _random = new();
        private ITestOutputHelper _output;

        public QueryDataConverterTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            // Arrange
            var serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new QueryDataConverter(),
                },
            };

            var jsonData = File.ReadAllText("TestData\\testquerydata-success-data.json");

            // Act
            var obj = JsonSerializer.Deserialize<TestQueryData>(jsonData, serializerOptions);

            // Assert
            Assert.NotNull(obj);

            foreach (var row in obj.Items)
            {
                foreach (var kv in row)
                {
                    _output.WriteLine("Column: " + kv.Key + ", Value: " + kv.Value + ", Type: " + kv.Value.GetType().ToString());
                }
            }
        }

        private class TestQueryData
        {
            public List<Dictionary<string, object?>> Items { get; set; } = new();
        }
    }
}