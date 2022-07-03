using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
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
