// <copyright file="ApplicationPermissions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace TheGrid.Shared.Constants
{
    /// <summary>
    /// Permissions available in the application.
    /// </summary>
    public enum ApplicationPermission
    {
        /// <summary>
        /// System Administrator.
        /// </summary>
        [Display(Name = "System Administrator", Description = "Can manage all organizations and manage access control in the system.")]
        SystemAdministrator,

        /// <summary>
        /// Organization Administrator.
        /// </summary>
        [Display(Name = "Organization Administrator", Description = "Can manage access control for a specific organization.")]
        OrganizationAdministrator,

        /// <summary>
        /// Invite Users.
        /// </summary>
        [Display(Name = "Invite Users", Description = "Can invite users to the organization.")]
        InviteUsers,

        /// <summary>
        /// Create Connections.
        /// </summary>
        [Display(Name = "Create Connections", Description = "Can create new connections.")]
        CreateConnection,

        /// <summary>
        /// Create Dashboards.
        /// </summary>
        [Display(Name = "Create Dashboards", Description = "Can create new dashboards.")]
        CreateDashboard,

        /// <summary>
        /// Create Queries.
        /// </summary>
        [Display(Name = "Create Queries", Description = "Can create new queries.")]
        CreateQuery,

        /// <summary>
        /// Create Alerts.
        /// </summary>
        [Display(Name = "Create Alerts", Description = "Can create new alerts.")]
        CreateAlert,

        /// <summary>
        /// Modify Connections.
        /// </summary>
        [Display(Name = "Modify Connections", Description = "Can modify connections.")]
        ModifyConnection,

        /// <summary>
        /// Modify Dashboards.
        /// </summary>
        [Display(Name = "Modify Dashboards", Description = "Can modify dashboards.")]
        ModifyDashboard,

        /// <summary>
        /// Modify Queries.
        /// </summary>
        [Display(Name = "Modify Queries", Description = "Can modify queries.")]
        ModifyQuery,

        /// <summary>
        /// Modify Alerts.
        /// </summary>
        [Display(Name = "Modify Alerts", Description = "Can modify alerts.")]
        ModifyAlert,

        /// <summary>
        /// Manage Folders.
        /// </summary>
        [Display(Name = "Manage Folders", Description = "Can manage folders.")]
        ManageFolders,

        /// <summary>
        /// Approve Connections.
        /// </summary>
        [Display(Name = "Approve Connections", Description = "Can approve connections creation/change requests.")]
        ApproveConnections,

        /// <summary>
        /// Approve Dashboards.
        /// </summary>
        [Display(Name = "Approve Dashboards", Description = "Can approve dashboard creation/change requests.")]
        ApproveDashboards,

        /// <summary>
        /// Approve Queries.
        /// </summary>
        [Display(Name = "Approve Queries", Description = "Can approve query creation/change requests.")]
        ApproveQueries,

        /// <summary>
        /// Approve Alerts.
        /// </summary>
        [Display(Name = "Approve Alerts", Description = "Can approve alert creation/change requests.")]
        ApproveAlerts,

        /// <summary>
        /// View Connections.
        /// </summary>
        [Display(Name = "View Connections", Description = "Can view connections.")]
        ViewConnection,

        /// <summary>
        /// View Dashboards.
        /// </summary>
        [Display(Name = "View Dashboards", Description = "Can view dashboards.")]
        ViewDashboard,

        /// <summary>
        /// View Queries.
        /// </summary>
        [Display(Name = "View Queries", Description = "Can view queries.")]
        ViewQuery,

        /// <summary>
        /// View Alerts.
        /// </summary>
        [Display(Name = "View Alerts", Description = "Can view alerts.")]
        ViewAlert,

        /// <summary>
        /// View Query Source.
        /// </summary>
        [Display(Name = "View Query Source", Description = "Can view query source.")]
        ViewQuerySource,

        /// <summary>
        /// Execute Queries.
        /// </summary>
        [Display(Name = "Execute Queries", Description = "Can execute queries.")]
        ExecuteQuery,
    }
}
