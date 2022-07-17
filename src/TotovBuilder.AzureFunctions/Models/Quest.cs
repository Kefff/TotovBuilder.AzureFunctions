namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a quest.
    /// </summary>
    public class Quest
    {
        /// <summary>
        /// ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Merchant.
        /// </summary>
        public string Merchant { get; set; } = string.Empty;

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Wiki link.
        /// </summary>
        public string WikiLink { get; set; } = string.Empty;
    }
}
