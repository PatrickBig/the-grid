// <copyright file="TheGridContextFactory.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using TheGrid.Models.Configuration;

namespace TheGrid.Data
{
    /// <summary>
    /// Database context factory.
    /// </summary>
    public class TheGridContextFactory : IDesignTimeDbContextFactory<TheGridDbContext>
    {
        /// <inheritdoc/>
        public TheGridDbContext CreateDbContext(string[] args)
        {
            var parentDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? throw new NullReferenceException("Unable to determine parent directory");
            var apiProjectProvider = new PhysicalFileProvider(Path.Combine(parentDirectory, "TheGrid.Server"));

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile(apiProjectProvider, "appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            var dataConfig = configuration.GetSection(nameof(SystemOptions)).Get<SystemOptions>();

            var builder = new DbContextOptionsBuilder<TheGridDbContext>();

            var connectionString = configuration.GetConnectionString("Database");

            if (dataConfig?.DatabaseProvider == DatabaseProvider.PostgreSql)
            {
#if INITIAL_MIGRATION
                builder.UseNpgsql(connectionString);
#else
                builder.UseNpgsql(connectionString, o => o.MigrationsAssembly("TheGrid.Postgres"));
#endif
            }
            else
            {
                throw new ArgumentException("Invalid provider name specified.");
            }

            return new TheGridDbContext(builder.Options);
        }
    }
}
