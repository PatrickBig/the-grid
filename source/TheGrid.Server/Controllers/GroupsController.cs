// <copyright file="GroupsController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Net.Mime;
using TheGrid.Data;
using TheGrid.Server.Extensions;
using TheGrid.Services;
using TheGrid.Shared.Constants;
using TheGrid.Shared.Extensions;
using TheGrid.Shared.Models;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Controller for managing groups.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    [Produces(MediaTypeNames.Application.Json)]
    public class GroupsController(TheGridDbContext dbContext, IGroupManager _groupManager) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> CreateGroupAsync([FromBody] CreateGroupRequest request, CancellationToken cancellationToken = default)
        {
            if (!CanAdministerGroups(request.OrganizationId))
            {
                return Unauthorized();
            }

            await _groupManager.CreateGroupAsync(request.Name, request.OrganizationId, request.Description, request.Permissions, cancellationToken);
            return Created();
        }



        /// <summary>
        /// Gets a list of groups available in the system.
        /// </summary>
        /// <param name="organizationId">ID of the organization to get the groups from.</param>
        /// <param name="sort">Sort to apply to the list of groups.</param>
        /// <param name="skip">Number of groups to skip in a paginated request.</param>
        /// <param name="take">Number of groups to take in the paginated request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paginated list of groups.</returns>
        [HttpGet]
        public async Task<ActionResult> GetGroupsAsync(
            [FromQuery][Required] string organizationId,
            [FromQuery] Sort[]? sort,
            [FromQuery] int skip = 0,
            [FromQuery][Range(1, 200)] int take = 25,
            CancellationToken cancellationToken = default)
        {
            if (!User.IsMemberOfOrganization(organizationId) && !User.IsInRole(GridRoles.SystemAdministrator))
            {
                return Unauthorized();
            }

            var baseQuery =
                from g in dbContext.Roles
                where g.OrganizationId == null || g.OrganizationId == organizationId
                select new GroupInformation
                {
                    GroupName = g.Name,
                    Description = g.Description,
                    IsBuiltIn = g.IsBuiltIn,

                    OrganizationId = g.OrganizationId,
                };

            if (sort != null && sort.Length != 0)
            {
                baseQuery = baseQuery.OrderBy(sort.GetSortStatement());
            }

            var resultQuery = baseQuery
                .Skip(skip)
                .Take(take);

            var result = new PaginatedResult<GroupInformation>
            {
                Items = await resultQuery.ToArrayAsync(cancellationToken),
                TotalItems = await baseQuery.CountAsync(cancellationToken),
            };

            return Ok(result);
        }

        private bool CanAdministerGroups(string? organizationId)
        {
            if (User.IsSystemAdministrator())
            {
                return true;
            }

            if (organizationId != null && User.HasPermission(organizationId, ApplicationPermission.OrganizationAdministrator))
            {
                return true;
            }

            return false;
        }
    }
}
