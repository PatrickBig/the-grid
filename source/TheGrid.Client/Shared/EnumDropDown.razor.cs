// <copyright file="EnumDropDown.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;
using TheGrid.Shared.Utilities;

namespace TheGrid.Client.Shared
{
    /// <summary>
    /// Dropdown list generated from the possible values in an enum.
    /// </summary>
    /// <typeparam name="TEnum">Type of enum to generate list from.</typeparam>
    public partial class EnumDropDown<TEnum> : ComponentBase
        where TEnum : struct, Enum
    {
        private IEnumerable<EnumSelectOption> _options = default!;

        /// <summary>
        /// Gets or sets what value type to bind to from the enum.
        /// </summary>
        public enum ValueBindingType
        {
            /// <summary>
            /// The bind value will be the name of the enum member.
            /// </summary>
            Name,

            /// <summary>
            /// The bind value will be the numeric value of the enum member.
            /// </summary>
            Numeric,
        }

        /// <summary>
        /// Gets the type type of value to bind to.
        /// </summary>
        [Parameter]
        public ValueBindingType ValueBinding { get; set; } = ValueBindingType.Name;

        /// <summary>
        /// Gets or sets a value indicating whether clearing the selected option is allowed.
        /// </summary>
        [Parameter]
        public bool AllowClear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multiple values can be selected.
        /// </summary>
        [Parameter]
        public bool Multiple { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the select displays the selected options as chips.
        /// </summary>
        [Parameter]
        public bool Chips { get; set; }

        /// <summary>
        /// Gets or sets the value of the selected option. This will always be null if <see cref="ValueBindingType"/> is <see cref="ValueBindingType.Numeric"/>.
        /// </summary>
        [Parameter]
        public string? NameValue { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is fired when the name value changes.
        /// </summary>
        [Parameter]
        public EventCallback<string?> NameValueChanged { get; set; }

        /// <summary>
        /// Gets or sets the value of the selected option. This will always be null if <see cref="ValueBindingType"/> is <see cref="ValueBindingType.Name"/>.
        /// </summary>
        [Parameter]
        public int? NumericValue { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is fired when the numeric value changes.
        /// </summary>
        [Parameter]
        public EventCallback<int?> NumericValueChanged { get; set; }

        private string ValuePropertyName => ValueBinding switch
        {
            ValueBindingType.Name => nameof(EnumSelectOption.Name),
            ValueBindingType.Numeric => nameof(EnumSelectOption.Value),
            _ => nameof(EnumSelectOption.Name),
        };

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            _options = EnumUtilities.GetSelectOptions<TEnum>();
        }
    }
}