// <copyright file="QueryColumnFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGrid.Shared.Models;

namespace TheGrid.Tests.Client
{
    /// <summary>
    /// Fixture class for defining column names used in queries.
    /// </summary>
    /// <remarks>
    /// This class contains constants for various column names used in query results.
    /// </remarks>
    internal static class QueryColumnFixture
    {
        /// <summary>
        /// The column name for an integer column.
        /// </summary>
        public const string IntegerColumnName = "IntegerColumn";

        /// <summary>
        /// The column name for a long column.
        /// </summary>
        public const string LongColumnName = "LongColumn";

        /// <summary>
        /// The column name for a boolean column.
        /// </summary>
        public const string BooleanColumnName = "BooleanColumn";

        /// <summary>
        /// The column name for a decimal column.
        /// </summary>
        public const string DecimalColumnName = "DecimalColumn";

        /// <summary>
        /// The column name for a string column.
        /// </summary>
        public const string StringColumnName = "StringColumn";

        /// <summary>
        /// The column name for a date column.
        /// </summary>
        public const string DateColumnName = "DateColumn";

        /// <summary>
        /// The column name for a time column.
        /// </summary>
        public const string TimeColumnName = "TimeColumn";

        /// <summary>
        /// The column name for a timestamp column.
        /// </summary>
        public const string TimestampColumnName = "TimestampColumn";

        /// <summary>
        /// The column name for a binary column.
        /// </summary>
        public const string BinaryColumnName = "BinaryColumn";

        /// <summary>
        /// The column name for a guid column.
        /// </summary>
        public const string GuidColumnName = "GuidColumn";

        /// <summary>
        /// The column name for a datetime column.
        /// </summary>
        public const string DateTimeColumnName = "DateTimeColumn";

        /// <summary>
        /// The column name for a text column.
        /// </summary>
        public const string TextColumnName = "TextColumn";

        /// <summary>
        /// Gets the columns used in query result tests.
        /// </summary>
        /// <returns>Test data used for columns.</returns>
        public static Dictionary<string, Column> GetColumns()
        {
            var columns = new Dictionary<string, Column>
            {
                {
                    IntegerColumnName, new Column
                    {
                        Type = QueryResultColumnType.Integer,
                    }
                },
                {
                    LongColumnName, new Column
                    {
                        Type = QueryResultColumnType.Long,
                    }
                },
                {
                    BooleanColumnName, new Column
                    {
                        Type = QueryResultColumnType.Boolean,
                    }
                },
                {
                    DecimalColumnName, new Column
                    {
                        Type = QueryResultColumnType.Decimal,
                    }
                },
                {
                    DateTimeColumnName, new Column
                    {
                        Type = QueryResultColumnType.DateTime,
                    }
                },
                {
                    TimeColumnName, new Column
                    {
                        Type = QueryResultColumnType.Time,
                    }
                },
                {
                    TextColumnName, new Column
                    {
                        Type = QueryResultColumnType.Text,
                    }
                },
            };

            return columns;
        }
    }
}
