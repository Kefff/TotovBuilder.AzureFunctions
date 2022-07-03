namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of eyewear.
    /// </summary>
    public interface IEyewear : IItem
    {
        /// <summary>
        /// Blindness protection percentage.
        /// </summary>
        public double BlindnessProtectionPercentage { get; set; }
    }
}
