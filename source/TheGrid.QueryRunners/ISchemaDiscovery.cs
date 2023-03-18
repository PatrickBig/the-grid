using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGrid.QueryRunners.Models;

namespace TheGrid.QueryRunners
{
    public interface ISchemaDiscovery
    {
        public Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default);
    }
}
