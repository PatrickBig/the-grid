namespace TheGrid.QueryRunners.Models
{
    public class QueryResults
    {
        public IEnumerable<string> Columns { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<Dictionary<string, object>> Rows { get; set; } = Enumerable.Empty<Dictionary<string, object>>();
    }
}