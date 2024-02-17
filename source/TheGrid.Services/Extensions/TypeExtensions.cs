// <copyright file="TypeExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Services.Extensions
{
    /// <summary>
    /// Type helpers.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Checks if the type implements the requested interface.
        /// </summary>
        /// <typeparam name="T">Type of interface to check for.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool ImplementsInterface<T>(this Type type)
        {
            return Array.Exists(type.GetInterfaces(), i => i == typeof(T));
        }
    }
}
