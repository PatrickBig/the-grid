using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.QueryRunners.Models
{
    public class DatabaseObjectField
    {
        public string Name { get; set; } = string.Empty;

        public string? TypeName { get; set; }

        public Dictionary<string, string>? Attributes { get; set; }
    }
}
