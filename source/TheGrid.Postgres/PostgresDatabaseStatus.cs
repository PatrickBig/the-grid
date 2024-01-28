// <copyright file="PostgresDatabaseStatus.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using TheGrid.Data;
using TheGrid.Models;

namespace TheGrid.Postgres
{
    /// <summary>
    /// Provides information about the Postgres database status.
    /// </summary>
    public class PostgresDatabaseStatus : IDatabaseStatus
    {
        private readonly TheGridDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDatabaseStatus"/> class.
        /// </summary>
        /// <param name="dbContext">Database context.</param>
        public PostgresDatabaseStatus(TheGridDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<DatabaseStatus> GetDatabaseStatusAsync(CancellationToken cancellationToken = default)
        {
            var query = FormattableStringFactory.Create($@"select 
            pg_table_size('""{nameof(TheGridDbContext.QueryResultRows)}""') as ""{nameof(DatabaseStatus.QueryResultCacheSize)}"",
            pg_database_size('{_dbContext.Database.GetDbConnection().Database}') as ""{nameof(DatabaseStatus.DatabaseSize)}""");

            var result = await _dbContext.Database.SqlQuery<DatabaseStatus>(query)
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException("Unable to fetch database status.");
            }

            return result;
        }
    }
}
