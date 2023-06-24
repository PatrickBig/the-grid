// <copyright file="TheGridContext.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using TheGrid.Models;
using TheGrid.QueryRunners.Models;

namespace TheGrid.Data
{
    /// <summary>
    /// Primary database context for solution. Provides data access layer to objects.
    /// </summary>
    public class TheGridDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TheGridDbContext"/> class.
        /// </summary>
        /// <param name="options">Options for the database context.</param>
        public TheGridDbContext(DbContextOptions<TheGridDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Data sources.
        /// </summary>
        public DbSet<DataSource> DataSources { get; set; }

        /// <summary>
        /// Organizations.
        /// </summary>
        public DbSet<Organization> Organizations { get; set; }

        /// <summary>
        /// Queries that can be executed by the context.
        /// </summary>
        public DbSet<Query> Queries { get; set; }

        /// <summary>
        /// Results from a query execution.
        /// </summary>
        public DbSet<QueryResultRow> QueryResultRows { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataSource>().HasIndex(d => d.OrganizationId);

            modelBuilder.Entity<Query>().HasIndex(q => q.DataSourceId);
            modelBuilder.Entity<Query>().Property(q => q.Columns)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Query>().Property(q => q.Parameters)
                .HasColumnType("jsonb");

            modelBuilder.Entity<QueryResultRow>().Property(r => r.Data)
                .HasColumnType("jsonb");

            modelBuilder.Entity<QueryResultRow>().HasIndex(q => q.QueryId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
