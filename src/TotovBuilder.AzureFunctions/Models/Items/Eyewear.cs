using TotovBuilder.AzureFunctions.Abstraction.Models.Items;

namespace TotovBuilder.AzureFunctions.Models.Items
{
    /// <summary>
    /// Represents eyewear.
    /// </summary>
    public class Eyewear : Item, IEyewear
    {
        /// <inheritdoc/>
        public double BlindnessProtectionPercentage { get; set; }
    }
}
