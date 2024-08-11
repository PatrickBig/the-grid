// <copyright file="IGroupManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models;
using TheGrid.Shared.Constants;

namespace TheGrid.Services
{
    /// <summary>
    /// Functions for managing groups.
    /// </summary>
    public interface IGroupManager
    {
        /// <summary>
        /// Creates a new system administrator group.
        /// </summary>
        /// <param name="name">Name of the administrator group.</param>
        /// <param name="description">A short description of the group.</param>
        /// <param name="builtIn">A flag indicating if the group is a built-in group.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns the newly created group.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user does not have permission to create a group.</exception>
        public Task<Group> CreateSystemAdministratorGroupAsync(string name, string? description, bool builtIn = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new group associated to an organization.
        /// </summary>
        /// <param name="name">Name for the group.</param>
        /// <param name="organizationId">Unique ID of the organization the group should be associated to.</param>
        /// <param name="description">A short description of the group.</param>
        /// <param name="permissions">Permissions the group should have.</param>
        /// <param name="builtIn">A flag indicating if the group is a built-in group.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns the newly created group.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user does not have permission to create a group.</exception>
        public Task<Group> CreateGroupAsync(string name, string organizationId, string? description, IEnumerable<ApplicationPermission> permissions, bool builtIn = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a group already exists for the given organization.
        /// </summary>
        /// <param name="name">The name of the group. This is case-insensitive.</param>
        /// <param name="organizationId">The unique ID of the organization the group belongs too.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns true if the group exists and the user has permission to view this group, otherwise false.</returns>
        public Task<bool> GroupExistsAsync(string name, string organizationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a system administrator group exists.
        /// </summary>
        /// <param name="name">The name of the group. This is case-insensitive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns true if the group exists, otherwise false.</returns>
        public Task<bool> SystemAdministratorGroupExistsAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches a single group from the database.
        /// </summary>
        /// <param name="groupId">Unique ID of the group to fetch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The group.</returns>
        /// <exception cref="ArgumentNullException">Thrown if there is no group with the given ID.</exception>
        public Task<Group> GetGroupAsync(int groupId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing group. This method cannot update built in groups.
        /// </summary>
        /// <param name="groupId">Unique ID of the group to be updated.</param>
        /// <param name="name">Name of the group.</param>
        /// <param name="description">Description for the group.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated group.</returns>
        /// <exception cref="ArgumentNullException">Thrown if there is no group with the given ID.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the group is a built-in group.</exception>
        public Task<Group> UpdateGroupInformationAsync(int groupId, string name, string? description, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the permissions for a group.
        /// </summary>
        /// <param name="groupId">Unique ID of the group to update the permissions for.</param>
        /// <param name="permissions">Permissions to assign to the group. Any permissions not in this will be removed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The permissions assigned to the group.</returns>
        /// <exception cref="ArgumentNullException">Thrown if there is no group with the given ID.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the group is a system administrator group.</exception>
        public Task<IEnumerable<ApplicationPermission>> UpdateGroupPermissionsAsync(int groupId, IEnumerable<ApplicationPermission> permissions, CancellationToken cancellationToken = default);
    }
}
