// <copyright file="IDataGenerator.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.TestHelpers.DataGenerators
{
    /// <summary>
    /// Generates test datasets.
    /// </summary>
    /// <typeparam name="TReturnType">The type returned from <see cref="GenerateData(int)"/>.</typeparam>
    public interface IDataGenerator<TReturnType> : IDisposable
    {
        /// <summary>
        /// Generate the specified number of records with varying types.
        /// </summary>
        /// <param name="numberOfRecords">Number of records to generate.</param>
        /// <returns>Object used to find the data once generated.</returns>
        public TReturnType GenerateData(int numberOfRecords);
    }
}
