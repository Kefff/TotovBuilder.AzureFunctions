namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a ricochet chance.
    /// </summary>
    public class RicochetChance
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Maximum value.
        /// </summary>
        public double XMaxValue { get; set; }

        /// <summary>
        /// Minimum value.
        /// </summary>
        public double XMinValue { get; set; }
    }
}
