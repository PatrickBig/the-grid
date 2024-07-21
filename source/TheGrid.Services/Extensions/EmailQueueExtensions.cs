// <copyright file="EmailQueueExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Services.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="IEmailQueue"/> interface.
    /// </summary>
    public static class EmailQueueExtensions
    {
        /// <summary>
        /// Sends an email to the specified recipient.
        /// </summary>
        /// <param name="emailQueue">Implementation of email queue.</param>
        /// <param name="bodyHtml">Body of the email to send.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="to">A single recipient in the "to" field of the email.</param>
        /// <param name="cancellationToken">Cancellation token. Note that this only cancels the process of adding the email to the queue. Once added to the queue an email will be sent.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task QueueEmailAsync(this IEmailQueue emailQueue, string bodyHtml, string subject, string to, CancellationToken cancellationToken = default)
        {
            return emailQueue.QueueEmailAsync(bodyHtml, subject, [to], Array.Empty<string>(), Array.Empty<string>(), cancellationToken);
        }

        /// <summary>
        /// Sends an email to the specified recipients.
        /// </summary>
        /// <param name="emailQueue">Implementation of email queue.</param>
        /// <param name="bodyHtml">Body of the email to send.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="to">The recipients in the "to" field of the email.</param>
        /// <param name="cancellationToken">Cancellation token. Note that this only cancels the process of adding the email to the queue. Once added to the queue an email will be sent.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task QueueEmailAsync(this IEmailQueue emailQueue, string bodyHtml, string subject, IEnumerable<string> to, CancellationToken cancellationToken = default)
        {
            return emailQueue.QueueEmailAsync(bodyHtml, subject, to, Array.Empty<string>(), Array.Empty<string>(), cancellationToken);
        }

        /// <summary>
        /// Sends an email to the specified recipients.
        /// </summary>
        /// <param name="emailQueue">Implementation of email queue.</param>
        /// <param name="bodyHtml">Body of the email to send.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="to">The recipients in the "to" field of the email.</param>
        /// <param name="cc">The recipients in the "cc" field of the email.</param>
        /// <param name="cancellationToken">Cancellation token. Note that this only cancels the process of adding the email to the queue. Once added to the queue an email will be sent.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task QueueEmailAsync(this IEmailQueue emailQueue, string bodyHtml, string subject, IEnumerable<string> to, IEnumerable<string> cc, CancellationToken cancellationToken = default)
        {
            return emailQueue.QueueEmailAsync(bodyHtml, subject, to, cc, Array.Empty<string>(), cancellationToken);
        }
    }
}
