// <copyright file="UserInformationController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Models;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Used to fetch information about a user.
    /// </summary>
    [Route("api/v{version:apiVersion}/userinfo")]
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    public class UserInformationController : ControllerBase
    {
        private readonly UserManager<GridUser> _userManager;
        private readonly TheGridDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInformationController"/> class.
        /// </summary>
        /// <param name="userManager">User manager.</param>
        /// <param name="db">Database context.</param>
        public UserInformationController(UserManager<GridUser> userManager, TheGridDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        /// <summary>
        /// Gets information about the currently logged in user.
        /// </summary>
        /// <returns>Returns information about the currently logged in user.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserInformationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            // Get the organizations of the user by accessing it from the database. I need to get the organization name and ID.
            var organizations = await _db.Organizations
                .Where(o => o.Users.Any(m => m.Id == user.Id))
                .Select(o => new UserOrganizationMembership
                { Name = o.Name, OrganizationId = o.Id })
                .ToListAsync();

            var response = new UserInformationResponse
            {
                Identifier = user.UserName ?? user.Id,
                DisplayName = user.DisplayName ?? user.UserName,
                Email = user.Email,
                Organizations = organizations,
            };

            response.Roles = await _userManager.GetRolesAsync(user);

            return Ok(response);
        }
    }
}
