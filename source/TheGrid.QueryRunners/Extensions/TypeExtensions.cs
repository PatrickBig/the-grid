using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.QueryRunners.Extensions
{
    public static class TypeExtensions
    {
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
