using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.QueryRunners.Models
{

    public class QueryParameter
    {
        public string Name { get; set; } = string.Empty;

        public object Value { get; set; } = string.Empty;
    }
}
