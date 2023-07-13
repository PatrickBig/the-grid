// <copyright file="QueryRunnerAttribute.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGrid.Shared.Models;

namespace TheGrid.QueryRunners.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class QueryRunnerAttribute : Attribute
    {
        public QueryRunnerAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Display name for the query runner.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Language used by the IDE / editor component. Most commonly languages are defined as constants on <see cref="Shared.Models.EditorLanguage"/>.
        /// </summary>
        public string? EditorLanguage { get; set; }

        /// <summary>
        /// Icon for the
        /// </summary>
        public string? IconFileName { get; set; } = "undefined.png";
    }
}
