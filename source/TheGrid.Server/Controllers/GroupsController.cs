// <copyright file="GroupsController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Net.Mime;
using TheGrid.Data;
using TheGrid.Server.Extensions;
using TheGrid.Server.Security;
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
    /// <param name="groupManager">Group manager.</param>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Policy = OrganizationHandler.PolicyName)]
    [Produces(MediaTypeNames.Application.Json)]
    public class GroupsController(TheGridDbContext dbContext, IGroupManager groupManager) : ControllerBase
    {
        private readonly IGroupManager _groupManager = groupManager;
        private readonly TheGridDbContext _dbContext = dbContext;

        /// <summary>
        /// Creates a new group for an organization.
        /// </summary>
        /// <param name="request">Details for the new group..</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Details about the newly created group.</returns>
        [HttpPost]
        public async Task<ActionResult> CreateGroup([FromBody] CreateGroupRequest request, CancellationToken cancellationToken = default)
        {
            if (!CanAdministerGroups(request.OrganizationId))
            {
                return Unauthorized();
            }

            var group = await _groupManager.CreateGroupAsync(request.Name, request.OrganizationId, request.Description, request.Permissions, false, cancellationToken);

            return CreatedAtAction(nameof(GetGroup), new { groupId = group.Id }, group.Adapt<GroupInformation>());
        }

        /// <summary>
        /// Gets information about a single group.
        /// Users must be a member of the organization that the group belongs to or a system administrator.
        /// </summary>
        /// <param name="groupId">Unique identifier of the group.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A status code indicating success or failure.</returns>
        [HttpGet("{groupId}")]
        public async Task<ActionResult> GetGroup([FromRoute] int groupId, CancellationToken cancellationToken = default)
        {
            var group = await _groupManager.GetGroupAsync(groupId, cancellationToken);

            // Make sure the user is authorized to see the group.
            if ((group.OrganizationId != null && !User.IsMemberOfOrganization(group.OrganizationId)) && !User.IsSystemAdministrator())
            {
                return Unauthorized();
            }

            return Ok(group.Adapt<GroupInformation>());
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
            [FromHeader(Name = "Organization-Id")][Required] string organizationId,
            [FromQuery] Sort[]? sort,
            [FromQuery] int skip = 0,
            [FromQuery][Range(1, 200)] int take = 25,
            CancellationToken cancellationToken = default)
        {
            var baseQuery =
                from g in _dbContext.Groups
                where g.OrganizationId == null || g.OrganizationId == organizationId
                select new GroupInformation
                {
                    Name = g.Name,
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
