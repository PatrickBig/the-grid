// <copyright file="UserManagerController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheGrid.Shared.Models;

namespace TheGrid.Server.Controllers
{
    [Route("api/v{version:apiVersion}/userinfo")]
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    public class UserInformationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UserInformationController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Gets information about the currently logged in user.
        /// </summary>
        /// <returns></returns>
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

            var response = new UserInformationResponse
            {
                Identifier = user.UserName,
                DisplayName = user.UserName, // TODO: Add a "display name" property
                Email = user.Email,
            };

            response.Roles = await _userManager.GetRolesAsync(user);

            // Build the response
            //response.Add(new Claim(ClaimTypes.OtherPhone, user.PhoneNumber ?? string.Empty));
            //response.Add(new Claim(nameof(user.TwoFactorEnabled), user.TwoFactorEnabled.ToString()));


            return Ok(response);
        }
    }
}
