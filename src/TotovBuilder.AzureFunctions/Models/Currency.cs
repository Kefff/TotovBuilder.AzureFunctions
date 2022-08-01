namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a currency.
    /// </summary>
    public class Currency
    {
        /// <summary>
        /// Icon name.
        /// </summary>
        public string IconName { get; set; } = string.Empty;

        /// <summary>
        /// ID of the item corresponding to the currency.
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the currency is the main currency.
        /// </summary>
        public bool MainCurrency { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
