// <copyright file="UserInformationResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response containing user information.
    /// </summary>
    public class UserInformationResponse
    {
        /// <summary>
        /// Gets or sets the unique ID of the user.
        /// </summary>
        /// <example><code>somebody@example.com</code></example>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        /// <example><code>John Smith</code></example>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        /// <example><code>somebody@example.com</code></example>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the organizations the user is a member of.
        /// </summary>
        public IList<UserOrganizationMembership> Organizations { get; set; } = Array.Empty<UserOrganizationMembership>();

        /// <summary>
        /// Gets or sets the default organization of the current user.
        /// </summary>
        public string? CurrentOrganizationId { get; set; }

        public List<UserClaim> Claims { get; set; } = new List<UserClaim>();

        public class UserClaim
        {
            public UserClaim()
            {

            }

            public UserClaim(string type, string value)
            {
                Type = type;
                Value = value;
            }

            public UserClaim(string type, string value, string issuer)
            {
                Type = type;
                Value = value;
                Issuer = issuer;
            }

            public string Type { get; set; } = string.Empty;

            public string Value { get; set; } = string.Empty;

            public string? Issuer { get; set; }
        }
    }
}
