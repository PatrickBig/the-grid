using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Services
{
    public class TheGridContextFactory : IDesignTimeDbContextFactory<TheGridContext>
    {
        public TheGridContext CreateDbContext(string[] args)
        {
            if (args != null && args.Length == 1)
            {
                var connectionString = args[0];
                var builder = new DbContextOptionsBuilder<TheGridContext>();
                builder.UseNpgsql(connectionString, o => o.MigrationsAssembly("TheGrid.Postgres"));
                //builder.UseNpgsql(connectionString);

                return new TheGridContext(builder.Options);
            }
            else
            {
                throw new ArgumentException("Missing connection string for migration builder.");
            }
        }
    }
}
