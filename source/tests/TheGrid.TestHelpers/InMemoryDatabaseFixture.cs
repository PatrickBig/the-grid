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
    public sealed class InMemoryDatabaseFixture : IDisposable
    {
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
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
