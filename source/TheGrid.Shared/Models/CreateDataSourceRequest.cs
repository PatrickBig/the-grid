using System.ComponentModel.DataAnnotations;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Request to create a new data source.
    /// </summary>
    public class CreateDataSourceRequest
    {
        /// <summary>
        /// The name of the connection string.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The connection string used by the query runner to execute.
        /// </summary>
        /// <remarks>
        /// This value is encrypted in the database when stored.
        /// Most query runners support storing the password and username in <see cref="Properties"/>.
        /// If your query runner supports this functionality it is recommended to not include a user/pass in the connection string.
        /// </remarks>
        [Required]
        [StringLength(300)]
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the organization the data source is being created for.
        /// </summary>
        /// <remarks>
        /// This ID must be valid and have a corresponding Organization.
        /// </remarks>
        [Required]
        public int OrganizationId { get; set; }

        /// <summary>
        /// Extra properties passed to the query runner. This often contains username, password, etc.
        /// </summary>
        /// <remarks>
        /// This value is encrypted in the database when stored.
        /// </remarks>
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
