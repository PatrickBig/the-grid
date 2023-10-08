// <copyright file="TheGridDbContext.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using TheGrid.Models;
using TheGrid.Models.Visualizations;
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
        /// Connectors used to execute queries.
        /// </summary>
        public DbSet<Connector> Connectors { get; set; }

        /// <summary>
        /// ColumnOptions discovered from a query execution.
        /// </summary>
        public DbSet<Column> QueryColumns { get; set; }

        /// <summary>
        /// Visualizations for the queries.
        /// </summary>
        public DbSet<Visualization> Visualizations { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Connection>().HasIndex(d => d.OrganizationId);

            //modelBuilder.Entity<Query>().HasIndex(q => q.ConnectionId);

            // Probably don't want this one at all
            //modelBuilder.Entity<Query>().Property(q => q.ColumnOptions)
            //    .HasColumnType("jsonb");

            //modelBuilder.Entity<Query>().Property(q => q.Parameters)
            //    .HasColumnType("jsonb");

            modelBuilder.Entity<QueryResultRow>().Property(r => r.Data)
                .HasColumnType("jsonb");

            //modelBuilder.Entity<QueryResultRow>().HasIndex(q => q.QueryId);

            modelBuilder.Entity<Connector>().Property(r => r.Parameters)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Column>().HasKey(c => new { c.QueryId, c.Name });

            //modelBuilder.Entity<Visualization>()
            //    .HasDiscriminator<string>("VisualizationType")
            //    .HasValue<Visualization>
            modelBuilder.Entity<TableVisualization>()
                .HasBaseType<Visualization>();

            modelBuilder.Entity<TableVisualization>()
                .Property(r => r.Columns)
                .HasColumnType("jsonb");

            base.OnModelCreating(modelBuilder);
        }
    }
}
