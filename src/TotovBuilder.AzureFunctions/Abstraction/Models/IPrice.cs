namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of a price.
    /// </summary>
    public interface IPrice
    {
        /// <summary>
        /// Name of the currency.
        /// </summary>
        public string CurrencyName { get; set; }

        /// <summary>
        /// Merchant.
        /// </summary>
        public string? Merchant { get; set; }

        /// <summary>
        /// Merchant level.
        /// </summary>
        public double MerchantLevel { get; set; }

        /// <summary>
        /// Requires a quest.
        /// </summary>
        public bool RequiresQuest { get; set; }

        /// <summary>
        /// Value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Value in main currency
        /// </summary>
        public double ValueInMainCurrency { get; set; }
    }
}
