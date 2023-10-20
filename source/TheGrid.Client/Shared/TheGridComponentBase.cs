// <copyright file="TheGridComponentBase.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Radzen;

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
        protected NotificationService NotificationService { get; private set; } = null!;

        /// <summary>
        /// Organization for the current user.
        /// </summary>
        [CascadingParameter]
        protected UserOrganization UserOrganization { get; set; } = null!;

        /// <summary>
        /// Http client.
        /// </summary>
        [Inject]
        protected HttpClient HttpClient { get; set; } = null!;

        /// <summary>
        /// Disposes of the cancellation token.
        /// </summary>
        public virtual void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
