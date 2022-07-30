using System;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a price.
    /// </summary>
    public class Price
    {
        /// <summary>
        /// Barter items.
        /// </summary>
        public BarterItem[] BarterItems { get; set; } = Array.Empty<BarterItem>();

        /// <summary>
        /// Name of the currency.
        /// </summary>
        public string CurrencyName { get; set; } = string.Empty;

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
        public string? QuestId { get; set; }

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
