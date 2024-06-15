// <copyright file="TheGridDbContext.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
    public class TheGridDbContext(DbContextOptions<TheGridDbContext> options) : IdentityDbContext<GridUser>(options)
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

        /// <summary>
        /// Many-to-many relationship between users and organizations.
        /// </summary>
        public virtual DbSet<Models.UserOrganization> UserOrganizations { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<QueryResultRow>()
                .Property(r => r.Data)
                .HasConversion<JsonColumnConverter<Dictionary<string, object?>>>();

            builder.Entity<Connector>()
                .Property(r => r.Parameters)
                .HasConversion<JsonColumnConverter<List<ConnectionProperty>>>();

            builder.Entity<TheGrid.Models.Column>().HasKey(c => new { c.QueryId, c.Name });

            builder.Entity<TableVisualization>()
                .HasBaseType<Visualization>();

            builder.Entity<TableVisualization>()
                .Property(r => r.Columns)
                .HasConversion<JsonColumnConverter<Dictionary<string, TableColumn>>>();

            builder.Entity<Connection>()
                .Property(c => c.ConnectionProperties)
                .HasConversion<JsonColumnConverter<Dictionary<string, string?>>>();

            // Setup the many-to-many for users and organizations
            builder.Entity<Models.UserOrganization>()
                .HasKey(ou => new { ou.UserId, ou.OrganizationId });

            builder.Entity<Organization>()
                .HasMany(o => o.Users)
                .WithMany(u => u.Organizations)
                .UsingEntity<Models.UserOrganization>();

            // Set up default organization for users who have one set.
            builder.Entity<GridUser>()
                .HasOne(u => u.DefaultOrganization);

            base.OnModelCreating(builder);
        }
    }
}
