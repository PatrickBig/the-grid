using Microsoft.EntityFrameworkCore;
using TheGrid.Models;
using TheGrid.QueryRunners.Models;

namespace TheGrid.Services
{
    public class TheGridContext : DbContext
    {
        public TheGridContext(DbContextOptions<TheGridContext> options) : base(options)
        {
        }

        public DbSet<DataSource> DataSources { get; set; }

        public DbSet<Organization> Organizations { get; set; }
    }
}
