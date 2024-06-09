// <copyright file="SeedData.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGrid.Shared.Constants;

namespace TheGrid.Services.Jobs
{
    public class SeedData : BackgroundService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SeedData> _logger;

        public SeedData(RoleManager<IdentityRole> roleManager, ILogger<SeedData> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SeedRolesAsync();
        }

        private async Task SeedRolesAsync()
        {
            _logger.LogTrace("Seeding Roles");
            if (!(await _roleManager.RoleExistsAsync(GridRoles.SystemAdministrator)))
            {
                _logger.LogTrace("Creating System Administrator Role");
                var role = await _roleManager.CreateAsync(new IdentityRole(GridRoles.SystemAdministrator));
            }
        }
    }
}
