// <copyright file="TheGridDbContext.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using TheGrid.Connectors.Models;
using TheGrid.Models;
using TheGrid.Shared.Models;

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
        /// Connections to various data sources.
        /// </summary>
        public DbSet<Connection> Connections { get; set; }

        /// <summary>
        /// Organizations.
        /// </summary>
        public DbSet<Organization> Organizations { get; set; }

        /// <summary>
        /// Queries that can be executed by the context.
        /// </summary>
        public DbSet<Query> Queries { get; set; }

        /// <summary>
        /// Execution history of queries.
        /// </summary>
        public DbSet<QueryExecution> QueryExecutions { get; set; }

        /// <summary>
        /// Results from a query execution.
        /// </summary>
        public DbSet<QueryResultRow> QueryResultRows { get; set; }

        /// <summary>
        /// connectors.
        /// </summary>
        public DbSet<Connector> Connectors { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Connection>().HasIndex(d => d.OrganizationId);

            //modelBuilder.Entity<Query>().HasIndex(q => q.DataSourceId);

            // Probably don't want this one at all
            //modelBuilder.Entity<Query>().Property(q => q.Columns)
            //    .HasColumnType("jsonb");

            //modelBuilder.Entity<Query>().Property(q => q.Parameters)
            //    .HasColumnType("jsonb");

            //modelBuilder.Entity<QueryResultRow>().Property(r => r.Data)
            //    .HasColumnType("jsonb");

            //modelBuilder.Entity<QueryResultRow>().HasIndex(q => q.QueryId);

            //modelBuilder.Entity<Connector>().Property(r => r.Parameters)
            //    .HasColumnType("jsonb");

            base.OnModelCreating(modelBuilder);
        }
    }
}
