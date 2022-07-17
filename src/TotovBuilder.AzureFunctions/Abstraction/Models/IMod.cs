namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of a mod.
    /// </summary>
    public interface IMod : IModdable
    {
        /// <summary>
        /// Modifier added to the weapon ergonomics.
        /// </summary>
        double ErgonomicsModifier { get; set; }
    }
}
