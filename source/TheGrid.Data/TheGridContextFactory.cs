// <copyright file="TheGridContextFactory.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

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
            var apiProjectDirectory = Path.Combine(parentDirectory, "TheGrid.Api");
            var apiProjectProvider = new PhysicalFileProvider(Path.Combine(parentDirectory, "TheGrid.Api"));

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile(apiProjectProvider, "appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            var dataConfig = configuration.GetSection(nameof(DataLayerOptions)).Get<DataLayerOptions>();

            var builder = new DbContextOptionsBuilder<TheGridDbContext>();

            if (dataConfig?.DatabaseProvider == DatabaseProviders.PostgreSql)
            {
                // builder.UseNpgsql(dataConfig.ConnectionString);
                builder.UseNpgsql(dataConfig.ConnectionString, o => o.MigrationsAssembly("TheGrid.Postgres"));
            }
            else
            {
                throw new ArgumentException("Invalid provider name specified.");
            }

            return new TheGridDbContext(builder.Options);
        }
    }
}
