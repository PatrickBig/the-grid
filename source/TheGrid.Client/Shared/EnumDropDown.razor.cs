// <copyright file="EnumDropDown.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;
using TheGrid.Shared.Utilities;

namespace TheGrid.Client.Shared
{
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
            Value,
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

        private string ValuePropertyName => ValueBinding switch
        {
            ValueBindingType.Name => nameof(EnumSelectOption.Name),
            ValueBindingType.Value => nameof(EnumSelectOption.Value),
            _ => nameof(EnumSelectOption.Name),
        };

        private Type ValueType => ValueBinding switch
        {
            ValueBindingType.Name => typeof(string),
            ValueBindingType.Value => typeof(int),
            _ => typeof(string),
        };

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            _options = EnumUtilities.GetSelectOptions<TEnum>();
        }
    }
}