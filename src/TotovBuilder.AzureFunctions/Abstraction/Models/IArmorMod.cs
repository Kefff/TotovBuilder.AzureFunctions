namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of an armor mod.
    /// </summary>
    public interface IArmorMod: IArmor, IModdable
    {
        /// <summary>
        /// Blindness protection percentage.
        /// </summary>
        public double BlindnessProtectionPercentage { get; set; }
    }
}
