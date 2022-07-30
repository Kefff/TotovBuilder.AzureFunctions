namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a barter item.
    /// </summary>
    public class BarterItem
    {
        /// <summary>
        /// ID.
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// Quantity.
        /// </summary>
        public double Quantity { get; set; }
    }
}
