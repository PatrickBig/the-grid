// <copyright file="GroupInformation.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Shared.Models
{
    public class GroupInformation
    {
        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        public string? GroupName { get; set; }

        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        [StringLength(250)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the organization identifier associated to the role.
        /// </summary>
        public string? OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role is built in and should only be managed by the system.
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is a system administrator.
        /// </summary>
        public bool SystemAdministrator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is an organization administrator.
        /// </summary>
        public bool OrganizationAdministrator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can invite other users to the organization.
        /// </summary>
        public bool InviteUsers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can create connections.
        /// </summary>
        public bool CreateConnection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can create dashboards.
        /// </summary>
        public bool CreateDashboard { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can create queries.
        /// </summary>
        public bool CreateQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can create alerts.
        /// </summary>
        public bool CreateAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can modify connections.
        /// </summary>
        public bool ModifyConnection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can modify dashboards.
        /// </summary>
        public bool ModifyDashboard { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can modify queries.
        /// </summary>
        public bool ModifyQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can modify alerts.
        /// </summary>
        public bool ModifyAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can manage folders.
        /// </summary>
        public bool ManageFolders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can approve connections.
        /// </summary>
        public bool ApproveConnections { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can approve dashboards.
        /// </summary>
        public bool ApproveDashboards { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can approve changes to queries.
        /// </summary>
        public bool ApproveQueries { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can approve changes to alerts.
        /// </summary>
        public bool ApproveAlerts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can view queries directly.
        /// </summary>
        public bool ViewQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can view query source code / definition.
        /// </summary>
        public bool ViewQuerySource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can execute queries.
        /// </summary>
        public bool ExecuteQuery { get; set; }
    }
}
