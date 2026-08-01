// <copyright file="GridUserEmailSender.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Stubble.Core.Interfaces;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheGrid.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Sends emails for users to do things like confirm their email address, reset password, etc.
    /// </summary>
    public class GridUserEmailSender : IEmailSender<GridUser>
    {
        private static readonly Dictionary<string, string> _templates = [];
        private readonly ILogger<GridUserEmailSender> _logger;
        private readonly IAsyncStubbleRenderer _stubble;
        private readonly IEmailQueue _emailQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridUserEmailSender"/> class.
        /// </summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="stubble">Stubble renderer.</param>
        /// <param name="emailQueue">Email queue.</param>
        public GridUserEmailSender(ILogger<GridUserEmailSender> logger, IAsyncStubbleRenderer stubble, IEmailQueue emailQueue)
        {
            _logger = logger;
            _stubble = stubble;
            _emailQueue = emailQueue;
        }

        /// <inheritdoc/>
        public async Task SendConfirmationLinkAsync(GridUser user, string email, string confirmationLink)
        {
            _logger.LogInformation("Sending confirmation email to user {AspNetUserId}.", user.Id);

            // Create the message
            var subject = "Confirm your email address";

            var parameters = new Dictionary<string, object?>
            {
                { nameof(confirmationLink), confirmationLink },
            };
            var messageBody = await GetEmailBodyAsync(parameters);

            // Queue the send email job
            await _emailQueue.QueueEmailAsync(messageBody, subject, email);
        }

        /// <inheritdoc/>
        public async Task SendPasswordResetCodeAsync(GridUser user, string email, string resetCode)
        {
            _logger.LogInformation("Sending password reset code to user {AspNetUserId}.", user.Id);

            // Create the message
            var subject = "Reset your password";

            var parameters = new Dictionary<string, object?>
            {
                { nameof(resetCode), resetCode },
            };

            var messageBody = await GetEmailBodyAsync(parameters);

            // Queue the send email job
            await _emailQueue.QueueEmailAsync(messageBody, subject, email);
        }

        /// <inheritdoc/>
        public async Task SendPasswordResetLinkAsync(GridUser user, string email, string resetLink)
        {
            _logger.LogInformation("Sending password reset link to user {AspNetUserId}.", user.Id);

            // Create the message
            var subject = "Reset your password";

            var parameters = new Dictionary<string, object?>
            {
                { nameof(resetLink), resetLink },
            };

            var messageBody = await GetEmailBodyAsync(parameters);

            // Queue the send email job
            await _emailQueue.QueueEmailAsync(messageBody, subject, email);
        }

        private async Task<string> GetEmailBodyAsync(Dictionary<string, object?> parameters, [CallerMemberName] string callerName = "")
        {
            // Remove the "Async" suffix from the caller name and append the ".html" extension.
            var templateName = callerName.Substring(0, callerName.Length - 5);

            if (!_templates.TryGetValue(templateName, out string? value))
            {
                // Read the template from disk
                var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new FileNotFoundException("Could not find the current directory.");
                var template = await File.ReadAllTextAsync(Path.Combine(currentDirectory, "Templates", "UserManagement", templateName + ".html"));
                value = template;
                _templates[templateName] = value;
            }

            return await _stubble.RenderAsync(value, parameters);
        }
    }
}
