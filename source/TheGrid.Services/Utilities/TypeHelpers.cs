// <copyright file="TypeHelpers.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Reflection;
using TheGrid.Connectors;

namespace TheGrid.Services
{
    /// <summary>
    /// Type helpers.
    /// </summary>
    public static class TypeHelpers
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

        /// <summary>
        /// Gets the types that implement IConnector.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns>A list of types.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the assembly is null.</exception>
        public static IEnumerable<T> GetEnumerableOfType<T>()
            where T : class, IConnector
        {
            var assembly = Assembly.GetAssembly(typeof(T));

            if (assembly == null)
            {
                throw new InvalidOperationException("Unable to locate assembly.");
            }
            else
            {
                return GetEnumerableOfTypeIterator<T>(assembly);
            }
        }

        private static IEnumerable<T> GetEnumerableOfTypeIterator<T>(Assembly assembly)
            where T : class, IConnector
        {
            foreach (var type in GetTargetTypes<T>(assembly))
            {
                var instance = (T?)Activator.CreateInstance(type);

                if (instance != null)
                {
                    yield return instance;
                }
            }
        }

        private static IEnumerable<Type> GetTargetTypes<T>(Assembly assembly) => assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(T)));
    }
}
