namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response message after creating a new organization.
    /// </summary>
    public class CreateOrganizationResponse
    {
        /// <summary>
        /// The unique identifier of the newly created organization.
        /// </summary>
        public int OrganizationId { get; set; }
    }
}
