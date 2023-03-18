using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGrid.QueryRunners.Models;

namespace TheGrid.QueryRunners
{
    public abstract class QueryRunnerBase : IQueryRunner
    {
        protected ConnectionConfiguration Configuration { get; set; }
        public abstract string Name { get; }

        public QueryRunnerBase(ConnectionConfiguration configuration)
        {
            Configuration = configuration;
        }

        public abstract Task<QueryResults> RunQueryAsync(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

        public abstract Task TestConnectionAsync(CancellationToken cancellationToken = default);
        public abstract AboutQueryRunner GetConnectionProperties();
    }
}
