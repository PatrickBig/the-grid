using TheGrid.QueryRunners.Models;

namespace TheGrid.Models
{
    public class DataSource
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty;

        public int OrganizationId { get; set; }

        public Organization Organization { get; set; } = new();

        public Dictionary<string, string> Properties { get; set; } = new();
    }
}