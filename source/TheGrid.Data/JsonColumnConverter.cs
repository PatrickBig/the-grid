// <copyright file="JsonColumnConverter.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace TheGrid.Data
{
    /// <summary>
    /// Converts a base type to a Json value stored in a string column.
    /// </summary>
    /// <typeparam name="TValue">Type of value to convert.</typeparam>
    public class JsonColumnConverter<TValue> : ValueConverter<TValue, string>
        where TValue : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonColumnConverter{TValue}"/> class.
        /// </summary>
        public JsonColumnConverter()
            : base(c => ToString(c), c => FromString(c))
        {
        }

        private static string ToString(TValue? value)
        {
            return JsonSerializer.Serialize(value);
        }

        private static TValue FromString(string value)
        {
            return JsonSerializer.Deserialize<TValue>(value) ?? new TValue();
        }
    }
}
