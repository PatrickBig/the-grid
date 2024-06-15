// <copyright file="LoginRequest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace TheGrid.Client.Models.User
{
    /// <summary>
    /// Information required to login.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Gets or sets the email address used to log in.
        /// </summary>
        [Required]
        [StringLength(256, MinimumLength = 1)]
        public string Email { get; set; } = default!;

        /// <summary>
        /// Gets or sets the password used to log in.
        /// </summary>
        [Required]
        [StringLength(256, MinimumLength = 1)]
        public string Password { get; set; } = default!;

        /// <summary>
        /// Gets or sets the two factor code. This is required if two factor authentication is enabled.
        /// </summary>
        public string? TwoFactorCode { get; set; }

        /// <summary>
        /// Gets or sets the recovery code. This is required if two factor authentication is enabled.
        /// </summary>
        public string? TwoFactorRecoveryCode { get; set; }
    }
}
