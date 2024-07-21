// <copyright file="EmailOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models.Configuration
{
    /// <summary>
    /// Email configuration options.
    /// </summary>
    public class EmailOptions
    {
        /// <summary>
        /// Gets or sets the email address to send emails from.
        /// </summary>
        public string FromAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the address to send emails from. If left null, the <see cref="FromAddress"/> will be used.
        /// </summary>
        public string? FromName { get; set; }

        /// <summary>
        /// Gets or sets the host of the SMTP server.
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the port of the SMTP server.
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL should be used when connecting to the SMTP server.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Gets or sets the username of the SMTP server.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password of the SMTP server.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
