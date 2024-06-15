// <copyright file="TheGridComponentBase.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Radzen;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared
{
    /// <summary>
    /// Base component that contains commonly used functionality.
    /// </summary>
    public abstract class TheGridComponentBase : ComponentBase, IDisposable
    {
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Cancellation token.
        /// </summary>
        protected CancellationToken CancellationToken
        {
            get
            {
                if (_cancellationTokenSource == null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                return _cancellationTokenSource.Token;
            }
        }

        /// <summary>
        /// Notification service.
        /// </summary>
        [Inject]
        protected NotificationService NotificationService { get; private set; } = default!;

        /// <summary>
        /// Organization for the current user.
        /// </summary>
        [CascadingParameter]
        protected UserOrganizationMembership UserOrganization { get; set; } = default!;

        /// <summary>
        /// Http client.
        /// </summary>
        [Inject]
        protected HttpClient HttpClient { get; set; } = default!;

        /// <summary>
        /// Disposes of the cancellation token.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the cancellation token.
        /// </summary>
        /// <param name="disposing">Set to true to perform cleanup.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }
}
