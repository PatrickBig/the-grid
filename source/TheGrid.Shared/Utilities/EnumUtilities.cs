// <copyright file="EnumUtilities.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using TheGrid.Shared.Models;

namespace TheGrid.Shared.Utilities
{
    /// <summary>
    /// Helper methods for interacting with enums.
    /// </summary>
    public static class EnumUtilities
    {
        /// <summary>
        /// Gets all the potential values of an enum and presents them as select options.
        /// <para>
        /// If the value on an enum does not have a <see cref="DisplayAttribute"/>, the <see cref="EnumSelectOption.DisplayName"/> will be set to the value of the enum and "Humanized".
        /// If this behavior is undesired consider adding a <see cref="DisplayAttribute"/> to the enum value.
        /// </para>
        /// </summary>
        /// <typeparam name="TEnum">Enum type to get the available options from.</typeparam>
        /// <returns>All the available options from an enum.</returns>
        /// <exception cref="NotSupportedException">Thrown if the underlying type of the enum is not an integer.</exception>
        public static IEnumerable<EnumSelectOption> GetSelectOptions<TEnum>()
            where TEnum : struct, Enum
        {
            var type = typeof(TEnum);

            if (Enum.GetUnderlyingType(type) != typeof(int))
            {
                throw new NotSupportedException("TEnum must be an enum of type int");
            }

            return GetSelectOptionsIterator<TEnum>();
        }

        private static IEnumerable<EnumSelectOption> GetSelectOptionsIterator<TEnum>()
            where TEnum : struct, Enum
        {
            // Get the values of the enum
            var options = Enum.GetValues(typeof(TEnum));

            // Return the display attributes
            foreach (var option in options)
            {
                if (option is Enum e)
                {
                    yield return new EnumSelectOption(e);
                }
                else
                {
                    throw new InvalidCastException($"This member {option} is not an enum.");
                }
            }
        }
    }
}
