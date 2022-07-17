namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a barter item.
    /// </summary>
    public class BarterItem
    {
        /// <inheritdoc/>
        public string CurrencyName { get; set; } = "RUB";

        /// <inheritdoc/>
        public string ItemId { get; set; } = string.Empty;

        /// <inheritdoc/>
        public Price Price { get; set; } = new Price();

        /// <inheritdoc/>
        public int Quantity { get; set; }
    }
}
