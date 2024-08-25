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
    public class SetupHostedService(IGroupManager groupManager, UserManager<GridUser> userManager, ILogger<SetupHostedService> logger, TheGridDbContext dbContext) : IHostedService
    {
        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ApplyDatabaseMigrationsAsync();
            await SetupDefaultGroupsAsync();
            await SetupUsersAsync();

            Environment.Exit(0);
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ApplyDatabaseMigrationsAsync()
        {
            logger.LogInformation("Applying Database Migrations");
            await dbContext.Database.MigrateAsync();
        }

        private async Task SetupUsersAsync()
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

        private async Task SetupDefaultGroupsAsync()
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
