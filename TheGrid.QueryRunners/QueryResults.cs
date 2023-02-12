namespace TheGrid.QueryRunners
{
    public class QueryResults
    {
        public IEnumerable<string> Columns { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<Dictionary<string, object>>  Results { get; set; } = Enumerable.Empty<Dictionary<string, object>>();
    }
}