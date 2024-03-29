﻿// <copyright file="TheGridDbContext.cs" company="BiglerNet">
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
    /// <remarks>
    /// Initializes a new instance of the <see cref="TheGridDbContext"/> class.
    /// </remarks>
    /// <param name="options">Options for the database context.</param>
    public class TheGridDbContext(DbContextOptions<TheGridDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// Connections to various data sources.
        /// </summary>
        public virtual DbSet<Connection> Connections { get; set; }

        /// <summary>
        /// Organizations.
        /// </summary>
        public virtual DbSet<Organization> Organizations { get; set; }

        /// <summary>
        /// Queries that can be executed by the context.
        /// </summary>
        public virtual DbSet<Query> Queries { get; set; }

        /// <summary>
        /// Execution history of queries.
        /// </summary>
        public virtual DbSet<QueryExecution> QueryExecutions { get; set; }

        /// <summary>
        /// Results from a query execution.
        /// </summary>
        public virtual DbSet<QueryResultRow> QueryResultRows { get; set; }

        /// <summary>
        /// Connectors used to execute queries.
        /// </summary>
        public virtual DbSet<Connector> Connectors { get; set; }

        /// <summary>
        /// ColumnOptions discovered from a query execution.
        /// </summary>
        public virtual DbSet<Models.Column> QueryColumns { get; set; }

        /// <summary>
        /// Visualizations for the queries.
        /// </summary>
        public virtual DbSet<Visualization> Visualizations { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueryResultRow>()
                .Property(r => r.Data)
                .HasConversion<JsonColumnConverter<Dictionary<string, object?>>>();

            modelBuilder.Entity<Connector>()
                .Property(r => r.Parameters)
                .HasConversion<JsonColumnConverter<List<ConnectionProperty>>>();

            modelBuilder.Entity<TheGrid.Models.Column>().HasKey(c => new { c.QueryId, c.Name });

            modelBuilder.Entity<TableVisualization>()
                .HasBaseType<Visualization>();

            modelBuilder.Entity<TableVisualization>()
                .Property(r => r.Columns)
                .HasConversion<JsonColumnConverter<Dictionary<string, TableColumn>>>();

            modelBuilder.Entity<Connection>()
                .Property(c => c.ConnectionProperties)
                .HasConversion<JsonColumnConverter<Dictionary<string, string?>>>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
