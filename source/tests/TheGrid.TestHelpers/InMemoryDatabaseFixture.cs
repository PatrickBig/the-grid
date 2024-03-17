// <copyright file="InMemoryDatabaseFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using TheGrid.Data;

namespace TheGrid.Tests.Shared
{
    /// <summary>
    /// Provides an in memory database context for <see cref="TheGridDbContext"/>.
    /// </summary>
    public class InMemoryDatabaseFixture : IDisposable
    {
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDatabaseFixture"/> class.
        /// </summary>
        public InMemoryDatabaseFixture()
        {
            var databaseOptions = new DbContextOptionsBuilder<TheGridDbContext>()
                .UseInMemoryDatabase("TheGrid")
                .Options;

            Db = new TheGridDbContext(databaseOptions);
        }

        /// <summary>
        /// Gets the in memory database context.
        /// </summary>
        public TheGridDbContext Db { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs cleanup of resources.
        /// </summary>
        /// <param name="disposing">Set to true to perform dispose action.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Db?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
