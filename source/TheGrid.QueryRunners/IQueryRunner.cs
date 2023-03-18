using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGrid.QueryRunners.Models;

namespace TheGrid.QueryRunners
{
    public interface IQueryRunner
    {
        public string Name { get; }

        public Task<QueryResults> RunQueryAsync(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

        public Task TestConnectionAsync(CancellationToken cancellationToken = default);

        public AboutQueryRunner GetConnectionProperties();
    }
}
