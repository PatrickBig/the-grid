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

            // Get the organizations of the user by accessing it from the database.
            var organizations = await _db.Organizations
                .Where(o => o.Users.Any(m => m.Id == user.Id))
                .Select(o => new UserOrganizationMembership
                {
                    Name = o.Name,
                    OrganizationId = o.Id,
                })
                .ToListAsync();

            var response = new UserInformationResponse
            {
                Identifier = user.UserName ?? user.Id,
                DisplayName = user.DisplayName ?? user.UserName,
                Email = user.Email,
                Organizations = organizations,
                DefaultOrganizationId = user.DefaultOrganizationId,
            };

            response.Roles = await _userManager.GetRolesAsync(user);

            return Ok(response);
        }

        /// <summary>
        /// Changes the user's organization.
        /// </summary>
        /// <param name="organizationId">Unique ID of the organization to change to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns IActionResult based on the success of changing the user's organization.</returns>
        [HttpPut]
        [Route(nameof(ChangeOrganization))]
        public async Task<IActionResult> ChangeOrganization([FromQuery] string organizationId, CancellationToken cancellationToken = default)
        {
            // Check that the user is a member of the given organization.
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var userOrganization = await _db.UserOrganizations
                .Where(u => u.OrganizationId == organizationId && u.UserId == user.Id)
                .Include(u => u.Organization)
                .Select(u => new UserOrganizationMembership
                {
                    OrganizationId = u.OrganizationId,
                    Name = u!.Organization!.Name,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (userOrganization != null)
            {
                user.DefaultOrganizationId = organizationId;
                await _userManager.UpdateAsync(user);
                return Ok(userOrganization);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
