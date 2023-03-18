using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.QueryRunners.Models
{
    public class ConnectionConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;

        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
