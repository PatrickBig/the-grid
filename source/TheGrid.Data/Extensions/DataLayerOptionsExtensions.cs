// <copyright file="DataLayerOptionsExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Data.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DataLayerOptions"/>.
    /// </summary>
    public static class DataLayerOptionsExtensions
    {
        /// <summary>
        /// Validates that the options are valid and have enough information to connect.
        /// </summary>
        /// <param name="options">Options to validate.</param>
        /// <exception cref="NotImplementedException">Thrown when the database provider is not supported.</exception>
        /// <exception cref="ArgumentNullException">Thrown if no configuration for the data layer was found.</exception>
        public static void ValidateOptions(this DataLayerOptions? options)
        {
            if (options != null)
            {
                // As new database providers are added this block will need to be expanded.
                if (options.DatabaseProvider != DatabaseProviders.PostgreSql)
                {
                    throw new NotImplementedException($"The database provider {options.DatabaseProvider} is not supported.");
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(options), "No configuration for the data layer was found.");
            }
        }
    }
}
