namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents an item type.
    /// </summary>
    public class ItemType
    {
        /// <summary>
        /// ID of the type in Tarkov.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the type in Tarkov.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
