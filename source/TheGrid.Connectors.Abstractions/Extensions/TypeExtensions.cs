// <copyright file="TypeExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the corresponding <see cref="QueryResultColumnType"/> for a given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type of the value for the column.</param>
        /// <returns>The <see cref="QueryResultColumnType"/> for the given <see cref="Type"/>.</returns>
        public static QueryResultColumnType GetQueryResultColumnTypeForType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType switch
            {
                Type t when t == typeof(TimeSpan) => QueryResultColumnType.Time,
                Type t when t == typeof(DateTime) => QueryResultColumnType.DateTime,
                Type t when t == typeof(decimal) => QueryResultColumnType.Decimal,
                Type t when t == typeof(long) || t == typeof(uint) => QueryResultColumnType.Long,
                Type t when t == typeof(short) || t == typeof(ushort) || t == typeof(int) => QueryResultColumnType.Integer,
                Type t when t == typeof(bool) => QueryResultColumnType.Boolean,
                Type t when t == typeof(string) => QueryResultColumnType.Text,
                Type t when t == typeof(Guid) => QueryResultColumnType.Guid,
                Type t when t == typeof(byte[]) => QueryResultColumnType.Binary,
                Type t when t == typeof(System.Text.Json.JsonElement) || t == typeof(System.Text.Json.JsonDocument) => QueryResultColumnType.Json,
                _ => QueryResultColumnType.Unknown,
            };
        }
    }
}
