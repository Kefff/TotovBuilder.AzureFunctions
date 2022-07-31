namespace TotovBuilder.AzureFunctions.Abstraction.Models.Items
{
    /// <summary>
    /// Provides the functionalities of a magazine.
    /// </summary>
    public interface IMagazine : IContainer, IMod
    {
        /// <summary>
        /// IDs of accepted ammunition.
        /// </summary>
        string[] AcceptedAmmunitionIds { get; set; }

        /// <summary>
        /// Modifier added to the check speed in percentage.
        /// </summary>
        double CheckSpeedPercentageModifier { get; set; }

        /// <summary>
        /// Modifier added to the loading speed in percentage.
        /// </summary>
        double LoadSpeedPercentageModifier { get; set; }

        /// <summary>
        /// Malfunction chance in percentage.
        /// </summary>
        double MalfunctionPercentage { get; set; }
    }
}
