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
    internal class QueryColumnFixture
    {
        public const string IntegerColumnName = "IntegerColumn";
        public const string LongColumnName = "LongColumn";
        public const string BooleanColumnName = "BooleanColumn";
        public const string DecimalColumnName = "DecimalColumn";
        public const string DateTimeColumnName = "DateTimeColumn";
        public const string TimeColumnName = "TimeColumn";
        public const string TextColumnName = "TextColumn";

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
