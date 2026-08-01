// <copyright file="IEmailQueue.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Services
{
    /// <summary>
    /// Used to queue emails to the backend where they will be sent as a batch.
    /// </summary>
    public interface IEmailQueue
    {
        /// <summary>
        /// Sends an email to the specified recipients.
        /// </summary>
        /// <param name="bodyHtml">Body of the email to send.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="to">The recipients in the "to" field of the email.</param>
        /// <param name="cc">The recipients in the "cc" field of the email.</param>
        /// <param name="bcc">The recipients in the "bcc" field of the email.</param>
        /// <param name="cancellationToken">Cancellation token. Note that this only cancels the process of adding the email to the queue. Once added to the queue an email will be sent.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task QueueEmailAsync(string bodyHtml, string subject, IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc, CancellationToken cancellationToken = default);
    }
}