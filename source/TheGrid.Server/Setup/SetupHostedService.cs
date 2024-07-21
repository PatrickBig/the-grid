using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Server.Controllers;
using TheGrid.Shared.Constants;

namespace TheGrid.Server.Setup
{
    /// <summary>
    /// Hosted service that runs when the application starts with the `/setup` argument supplied.
    /// This applies any database migrations, seeds data, prepares the file system, and exits the application.
    /// </summary>
    public class SetupHostedService(RoleManager<GridRole> roleManager, UserManager<GridUser> userManager, ILogger<SetupHostedService> logger, TheGridDbContext dbContext) : IHostedService
    {
        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ApplyDatabaseMigrationsAsync();
            await SetupRolesAsync();
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
                    await userManager.AddToRoleAsync(adminUser, GridRoles.SystemAdministrator);
                }
                else
                {
                    logger.LogError("Unable to create System Administrator. Error = {Error}", string.Join(", ", result.Errors));
                }

            }
        }

        private async Task SetupRolesAsync()
        {
            logger.LogInformation("Seeding Roles");
            if (!(await roleManager.RoleExistsAsync(GridRoles.SystemAdministrator)))
            {
                logger.LogTrace("Creating System Administrator Role");

                var systemAdminRole = new GridRole(GridRoles.SystemAdministrator)
                {
                    Description = "System Administrator",
                    IsBuiltIn = true,
                };

                var role = await roleManager.CreateAsync(systemAdminRole);
                if (role.Succeeded)
                {
                    await roleManager.AddClaimAsync(systemAdminRole, new Claim(GridClaimTypes.Permission, ApplicationPermission.SystemAdministrator.ToString(), ClaimValueTypes.String, "system"));
                }
                else
                {
                    logger.LogError("Unable to create System Administrator Role. Error = {Error}", string.Join(", ", role.Errors));
                }
            }
        }
    }
}
