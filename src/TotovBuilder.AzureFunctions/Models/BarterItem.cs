namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a barter item.
    /// </summary>
    public class BarterItem
    {
        /// <inheritdoc/>
        public string CurrencyName { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string ItemId { get; set; } = string.Empty;

        /// <inheritdoc/>
        public Price Price { get; set; } = new Price();

        /// <inheritdoc/>
        public double Quantity { get; set; }
    }
}
