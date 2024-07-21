// <copyright file="Emailer.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Microsoft.Extensions.Logging;

namespace TheGrid.Services
{
    /// <summary>
    /// Implementation to add queue emails to send later.
    /// </summary>
    /// <param name="backgroundJobClient">Background job client.</param>
    /// <param name="logger">Logger instance.</param>
    public class Emailer(IBackgroundJobClient backgroundJobClient, ILogger<Emailer> logger) : IEmailQueue
    {
        /// <inheritdoc/>
        public Task QueueEmailAsync(string bodyHtml, string subject, IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc, CancellationToken cancellationToken = default)
        {
            // Note that this is a temporary solution. Ultimately this will drop a record into a database, and there will be a recurring job that sends pending emails.
            var jobId = backgroundJobClient.Enqueue<EmailSendJob>(e => e.SendEmailAsync(bodyHtml, subject, to, cc, bcc, CancellationToken.None));
            logger.LogInformation("Queued email job via job {JobId}", jobId);

            return Task.CompletedTask;
        }
    }
}
