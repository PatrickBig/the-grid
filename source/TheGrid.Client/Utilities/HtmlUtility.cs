// <copyright file="HtmlUtility.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text;

namespace TheGrid.Client.Utilities
{
    /// <summary>
    /// Utility methods for HTML parsing, saftey, etc.
    /// </summary>
    public static class HtmlUtility
    {
        /// <summary>
        /// Get a safe ID from a string to use as the id attribute on an HTML element.
        /// This removes all characters that are not alphanumeric, dash, or underscore.
        /// </summary>
        /// <param name="input">Input to be sanitized.</param>
        /// <returns>Sanitized ID.</returns>
        public static string GetSafeId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            input = input.ToLowerInvariant();

            var sb = new StringBuilder(input.Length);

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (char.IsLetter(c) || char.IsNumber(c) || c == '-' || c == '_')
                {
                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    sb.Append('-');
                }
            }

            return sb.ToString();
        }
    }
}
