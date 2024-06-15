// <copyright file="SetupController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheGrid.Shared.Constants;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Performs setup operations in the system. This should only be run once unless the system is reset.
    /// </summary>
    /// <param name="roleManager">Role manager.</param>
    /// <param name="logger">Logging instance.</param>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class SetupController(RoleManager<IdentityRole> roleManager, ILogger<SetupController> logger) : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ILogger<SetupController> _logger = logger;

        /// <summary>
        /// Initializes the default roles in the system.
        /// </summary>
        /// <returns>Result of the operation.</returns>
        [HttpGet]
        [Route("SetupRoles")]
        public async Task<IActionResult> SetupRoles()
        {
            _logger.LogTrace("Seeding Roles");
            if (!(await _roleManager.RoleExistsAsync(GridRoles.SystemAdministrator)))
            {
                _logger.LogTrace("Creating System Administrator Role");
                var role = await _roleManager.CreateAsync(new IdentityRole(GridRoles.SystemAdministrator));
                if (!role.Succeeded)
                {
                    _logger.LogError("Unable to create System Administrator Role. Error = {Error}", string.Join(", ", role.Errors));
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to create System Administrator Role");
                }
            }

            return Ok();
        }
    }
}
