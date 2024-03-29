// <copyright file="SqliteProvider.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using TheGrid.Data;

namespace TheGrid.TestHelpers.DataGenerators
{
    /// <summary>
    /// Creates a Sqlite database with migrations applied.
    /// </summary>
    public class SqliteProvider : IDisposable
    {
        private readonly string _databasePath;
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteProvider"/> class.
        /// </summary>
        public SqliteProvider()
        {
            _databasePath = Path.GetTempFileName();
            var databaseOptions = new DbContextOptionsBuilder<TheGridDbContext>()
                .UseSqlite("Data Source=" + _databasePath, o => o.MigrationsAssembly("TheGrid.Sqlite"))
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .Options;

            Db = new TheGridDbContext(databaseOptions);

            Db.Database.Migrate();
        }

        /// <summary>
        /// Gets the database context created using the Sqlite provider. This database will already have migrations applied.
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
        /// Disposes of resources created by this class.
        /// </summary>
        /// <param name="disposing">Set to true to dispose of resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Db.Database.EnsureDeleted();

                    if (File.Exists(_databasePath))
                    {
                        File.Delete(_databasePath);
                    }
                }

                _disposedValue = true;
            }
        }
    }
}
