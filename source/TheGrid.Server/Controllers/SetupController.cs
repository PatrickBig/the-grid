// <copyright file="SetupController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheGrid.Services.Jobs;
using TheGrid.Shared.Constants;

namespace TheGrid.Server.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class SetupController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SetupController> _logger;

        public SetupController(RoleManager<IdentityRole> roleManager, ILogger<SetupController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [Route("SetupRoles")]
        public async Task<IActionResult> SetupRoles()
        {
            _logger.LogTrace("Seeding Roles");
            if (!(await _roleManager.RoleExistsAsync(TheGridRoles.SystemAdministrator)))
            {
                _logger.LogTrace("Creating System Administrator Role");
                var role = await _roleManager.CreateAsync(new IdentityRole(TheGridRoles.SystemAdministrator));
            }

            return Ok();
        }
    }
}
