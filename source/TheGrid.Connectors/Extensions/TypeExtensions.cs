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

            if (underlyingType == typeof(TimeSpan))
            {
                return QueryResultColumnType.Time;
            }
            else if (underlyingType == typeof(DateTime))
            {
                return QueryResultColumnType.DateTime;
            }
            else if (underlyingType == typeof(decimal))
            {
                return QueryResultColumnType.Decimal;
            }
            else if (underlyingType == typeof(long))
            {
                return QueryResultColumnType.Long;
            }
            else if (underlyingType == typeof(short)
                || underlyingType == typeof(ushort)
                || underlyingType == typeof(int))
            {
                return QueryResultColumnType.Integer;
            }
            else if (underlyingType == typeof(uint)
                || underlyingType == typeof(long))
            {
                return QueryResultColumnType.Long;
            }
            else if (underlyingType == typeof(bool))
            {
                return QueryResultColumnType.Boolean;
            }
            else
            {
                return QueryResultColumnType.Text;
            }
        }
    }
}
