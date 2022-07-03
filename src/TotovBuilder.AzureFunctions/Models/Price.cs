using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a price.
    /// </summary>
    public class Price : IPrice
    {
        /// <inheritdoc/>
        public string CurrencyName { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string? Merchant { get; set; }

        /// <inheritdoc/>
        public double MerchantLevel { get; set; }

        /// <inheritdoc/>
        public bool RequiresQuest { get; set; }

        /// <inheritdoc/>
        public double Value { get; set; }

        /// <inheritdoc/>
        public double ValueInMainCurrency { get; set; }
    }
}
