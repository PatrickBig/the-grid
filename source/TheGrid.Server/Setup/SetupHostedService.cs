// <copyright file="SetupHostedService.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Services;
using TheGrid.Shared.Constants;

namespace TheGrid.Server.Setup
{
    /// <summary>
    /// Hosted service that runs when the application starts with the `/setup` argument supplied.
    /// This applies any database migrations, seeds data, prepares the file system, and exits the application.
    /// </summary>
    /// <param name="serviceProvider">Root service provider, used to create a scope for resolving scoped dependencies.</param>
    /// <param name="lifetime">Application lifetime, used to defer setup work and to stop the application once complete.</param>
    /// <param name="logger">Logger instance.</param>
    public class SetupHostedService(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime, ILogger<SetupHostedService> logger) : IHostedService
    {
        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Kestrel's own hosted service starts alongside this one. Calling StopApplication()
            // (or exiting) synchronously from within StartAsync races with Kestrel's still-in-flight
            // BindAsync and can crash the process instead of stopping cleanly. Deferring to
            // ApplicationStarted runs setup after the host (and Kestrel) have fully finished
            // starting, so the application can shut down safely afterward.
            lifetime.ApplicationStarted.Register(() => _ = RunSetupAndStopAsync(cancellationToken));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task RunSetupAndStopAsync(CancellationToken cancellationToken)
        {
            try
            {
                // SetupHostedService is registered as a singleton (required by AddHostedService),
                // so scoped dependencies like TheGridDbContext/IGroupManager/UserManager must be
                // resolved from a short-lived scope rather than injected directly into the constructor.
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TheGridDbContext>();
                var groupManager = scope.ServiceProvider.GetRequiredService<IGroupManager>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GridUser>>();

                await ApplyDatabaseMigrationsAsync(dbContext);
                await SetupDefaultGroupsAsync(groupManager);
                await SetupUsersAsync(groupManager, userManager);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Setup failed.");
                Environment.ExitCode = 1;
            }
            finally
            {
                lifetime.StopApplication();
            }
        }

        private async Task ApplyDatabaseMigrationsAsync(TheGridDbContext dbContext)
        {
            logger.LogInformation("Applying Database Migrations");
            await dbContext.Database.MigrateAsync();
        }

        private async Task SetupUsersAsync(IGroupManager groupManager, UserManager<GridUser> userManager)
        {
            logger.LogInformation("Seeding default user");

            var user = await userManager.FindByNameAsync("admin");

            if (user == null)
            {
                logger.LogInformation("System administrator did not exist. Creating one.");

                var adminUser = new GridUser("admin")
                {
                    DisplayName = "System Administrator",
                };

                var defaultAdminPassword = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD") ?? throw new InvalidOperationException("Missing default password for built in system administrator. See documentation for fix.");

                var result = await userManager.CreateAsync(adminUser, defaultAdminPassword);

                if (result.Succeeded)
                {
                    // Add the user to the group
                    var systemAdminGroup = await groupManager.GetSystemAdministratorGroupByNameAsync(BuiltInGroups.SystemAdministrator);

                    if (systemAdminGroup == null)
                    {
                        throw new InvalidOperationException("System administrator group not found.");
                    }

                    await groupManager.AddUserToGroupAsync(systemAdminGroup.Id, adminUser.Id);

                    logger.LogInformation("System administrator was added to the default system admin group.");
                }
                else
                {
                    logger.LogError("Unable to create System Administrator. Error = {Error}", string.Join(", ", result.Errors));
                }
            }
        }

        private async Task SetupDefaultGroupsAsync(IGroupManager groupManager)
        {
            logger.LogInformation("Seeding default groups");
            if (!(await groupManager.SystemAdministratorGroupExistsAsync(BuiltInGroups.SystemAdministrator)))
            {
                logger.LogTrace("Creating System Administrator Role");

                await groupManager.CreateSystemAdministratorGroupAsync(BuiltInGroups.SystemAdministrator, "Default system administrator group", true);
            }
        }
    }
}
