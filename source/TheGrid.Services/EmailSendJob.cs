// <copyright file="EmailSendJob.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using TheGrid.Models.Configuration;

namespace TheGrid.Services
{
    /// <summary>
    /// Sends emails.
    /// </summary>
    public class EmailSendJob(IOptions<EmailOptions> emailOptions)
    {
        private readonly EmailOptions _emailOptions = emailOptions.Value;

        /// <summary>
        /// Sends an email.
        /// </summary>
        /// <param name="bodyHtml">Body of the email to send.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="to">The recipients in the "to" field of the email.</param>
        /// <param name="cc">The recipients in the "cc" field of the email.</param>
        /// <param name="bcc">The recipients in the "bcc" field of the email.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(string bodyHtml, string subject, IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc, CancellationToken cancellationToken = default)
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailOptions.SmtpHost, _emailOptions.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(_emailOptions.UserName, _emailOptions.Password, cancellationToken);

            var message = GetMimeMessage(bodyHtml, subject, to, cc, bcc);

            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }

        private MimeMessage GetMimeMessage(string bodyHtml, string subject, IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc)
        {
            var mimeMessage = new MimeMessage
            {
                Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = bodyHtml },
            };
            var from = new MailboxAddress(_emailOptions.FromName, _emailOptions.FromAddress);

            mimeMessage.From.Add(from);

            if (!to.Any())
            {
                throw new ArgumentException("No recipients were specified.", nameof(to));
            }

            mimeMessage.To.AddRange(to.Select(x => MailboxAddress.Parse(x)));

            if (cc.Any())
            {
                mimeMessage.Cc.AddRange(cc.Select(x => MailboxAddress.Parse(x)));
            }

            if (bcc.Any())
            {
                mimeMessage.Bcc.AddRange(bcc.Select(x => MailboxAddress.Parse(x)));
            }

            mimeMessage.Subject = subject;

            return mimeMessage;
        }
    }
}
