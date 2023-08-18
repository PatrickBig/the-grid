// <copyright file="TagsAttribute.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace TheGrid.Shared.Attributes
{
    /// <summary>
    /// <para>Validates that tags are in the allowed format.</para>
    /// <list type="bullet">
    /// <item>No more than 20 tags allowed.</item>
    /// <item>Each tag must be equal to or less than 30 characters in length.</item>
    /// <item>Each tag must contain only letters, numbers, spaces, hyphens, and underscores.</item>
    /// </list>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TagsAttribute : ValidationAttribute
    {
        private const int _maxNumberOfTags = 20;
        private const int _maxTagLength = 30;

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                if (value is IEnumerable<string> tags)
                {
                    if (tags.Count() > _maxNumberOfTags)
                    {
                        return new ValidationResult($"Cannot exceed {_maxNumberOfTags} tags.");
                    }

                    if (tags.Any(t => t.Length > _maxTagLength))
                    {
                        return new ValidationResult($"Tags cannot exceed {_maxTagLength} characters in length.");
                    }

                    if (tags.Any(t => !IsValidTag(t)))
                    {
                        return new ValidationResult("Invalid tag specified. Tags may only contain letters, numbers, spaces, hyphens, and underscores.");
                    }

                    return ValidationResult.Success;
                }
                else
                {
                    throw new ArgumentException($"The {nameof(TagsAttribute)} can only be used on {nameof(IEnumerable<string>)} properties");
                }
            }

            return base.IsValid(value, validationContext);
        }

        private static bool IsValidTag(string tag)
        {
            return tag.All(t => char.IsLetterOrDigit(t) || t is ' ' or '-' or '_');
        }
    }
}
