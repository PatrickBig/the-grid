namespace TheGrid.QueryRunners.Models
{
    public class DatabaseObject
    {
        /// <summary>
        /// Gets or sets the name of the database object.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// <para>Gets or sets a descriptive name for the object type.</para>
        /// <para>Values may be things like:</para>
        /// <list type="bullet">
        /// <item>Table</item>
        /// <item>View</item>
        /// <item>Collection</item>
        /// </list>
        /// </summary>
        public string ObjectTypeName { get; set; } = "Table";

        /// <summary>
        /// All of the fields/columns in this
        /// </summary>
        public IReadOnlyList<DatabaseObjectField>? Fields { get; set; }

        public Dictionary<string, string>? Attributes { get; set; }
    }
}
