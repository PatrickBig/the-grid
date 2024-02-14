// <copyright file="RelationalDatabaseDataGeneratorBase.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.TestHelpers.DataGenerators
{
    /// <summary>
    /// Base class for generating test data for relational database providers such as MS SQL, PostgreSql, Oracle, etc.
    /// </summary>
    public abstract class RelationalDatabaseDataGeneratorBase : IDataGenerator<string>
    {
        private bool _disposedValue;

        /// <summary>
        /// Generate the specified number of records with varying types.
        /// </summary>
        /// <param name="numberOfRecords">Number of records to generate.</param>
        /// <returns>Returns the name of the table that was created.</returns>
        public abstract string GenerateData(int numberOfRecords);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup datasets created by the generator.
        /// </summary>
        /// <param name="disposing">True if disposing of the managed objects.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CleanupData();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Clean all data that was created by the generator.
        /// </summary>
        protected abstract void CleanupData();
    }
}
