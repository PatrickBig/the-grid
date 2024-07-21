// <copyright file="EnumSelectOption.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Humanizer;
using System.ComponentModel.DataAnnotations;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Represents an option that came from some type of enum.
    /// </summary>
    public record EnumSelectOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumSelectOption"/> class.
        /// </summary>
        /// <param name="enumOption">Enum option to convert into a select option.</param>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="enumOption"/> is not of type <see cref="int"/>.</exception>
        public EnumSelectOption(Enum enumOption)
        {
            if (Enum.GetUnderlyingType(enumOption.GetType()) != typeof(int))
            {
                throw new NotSupportedException("Enum option must be of type int.");
            }

            Name = enumOption.ToString();
            Value = Convert.ToInt32(enumOption);

            var field = enumOption.GetType().GetField(enumOption.ToString()) ?? throw new InvalidOperationException("Unable to extract field from enum type.");

            if (field
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() is DisplayAttribute attribute)
            {
                DisplayName = attribute.GetName();
                Description = attribute.GetDescription();
                GroupName = attribute.GetGroupName();
                Order = attribute.GetOrder();
                Prompt = attribute.GetPrompt();
                ShortName = attribute.GetShortName();
            }
            else
            {
                DisplayName = enumOption.ToString().Humanize();
            }
        }

        /// <summary>
        /// Gets the name of the value from the enum.
        /// </summary>
        /// <remarks>
        /// For example, if an enum is defined as:
        /// <code>
        /// public enum MyEnum
        /// {
        ///     [Display(Name = "Option 1")]
        ///     Option1,
        /// }
        ///
        /// var option = new EnumSelectOption(MyEnum.Option1);
        /// </code>
        /// then <c>option.Name</c> will return <c>"Option1"</c>.
        /// </remarks>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the numeric value of the enum member.
        /// </summary>
        /// <remarks>
        /// For example, if an enum is defined as:
        /// <code>
        /// public enum MyEnum
        /// {
        ///     [Display(Name = "Option 1")]
        ///     Option1 = 1,
        /// }
        ///
        /// var option = new EnumSelectOption(MyEnum.Option1);
        /// </code>
        /// then <c>option.Value</c> will return <c>1</c>.
        /// </remarks>
        public int Value { get; private set; }

        /// <summary>
        /// Gets the display name of the enum member based on the <see cref="DisplayAttribute.Name"/> value. If this is not supplied the humanized version of the enum member name will be used.
        /// </summary>
        /// <remarks>
        /// For example, if an enum is defined as:
        /// <code>
        /// public enum MyEnum
        /// {
        ///     [Display(Name = "This is the first option available")]
        ///     FirstOption,
        ///
        ///     MyOtherOption
        /// }
        ///
        /// var firstOption = new EnumSelectOption(MyEnum.FirstOption);
        /// var secondOption = new EnumSelectOption(MyEnum.MyOtherOption);
        /// </code>
        /// then <c>option.DisplayName</c> will return <c>"This is the first option available"</c>.
        /// and <c>secondOption.DisplayName</c> will return <c>"My other option"</c>.
        /// </remarks>
        public string? DisplayName { get; private set; }

        /// <summary>
        /// Gets the description of the enum member from the <see cref="DisplayAttribute.Description"/> value. If not supplied this will return null.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Gets the group name of the enum member from the <see cref="DisplayAttribute.GroupName"/> value. If not supplied this will return null.
        /// </summary>
        public string? GroupName { get; private set; }

        /// <summary>
        /// Gets the order of the enum member from the <see cref="DisplayAttribute.Order"/> value. If not supplied this will return null.
        /// </summary>
        public int? Order { get; private set; }

        /// <summary>
        /// Gets the prompt of the enum member from the <see cref="DisplayAttribute.Prompt"/> value. If not supplied this will return null.
        /// </summary>
        public string? Prompt { get; private set; }

        /// <summary>
        /// Gets the short name of the enum member from the <see cref="DisplayAttribute.ShortName"/> value. If not supplied this will return null.
        /// </summary>
        public string? ShortName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the enum member is disabled.
        /// </summary>
        public bool IsDisabled { get; set; }
    }
}
