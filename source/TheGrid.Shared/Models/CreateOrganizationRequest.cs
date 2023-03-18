using System.ComponentModel.DataAnnotations;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Request to create a new organization.
    /// </summary>
    public class CreateOrganizationRequest
    {
        /// <summary>
        /// The short name / slug of the organization.
        /// </summary>
        /// <remarks>
        /// This must be a unique name in the system.
        /// </remarks>
        /// <example>default</example>
        [Required]
        [StringLength(20, MinimumLength = 3)]
        [RegularExpression("^[a-z]{1}[a-z0-9\\-]+$", ErrorMessage = "Must contain only lowercase a-z, numbers, and hyphens (-). Must start with a letter.")]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// The display name of the organization.
        /// </summary>
        /// <example>My Company Inc.</example>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
    }
}