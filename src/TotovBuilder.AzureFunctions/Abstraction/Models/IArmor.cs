namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of an armor.
    /// </summary>
    public interface IArmor : IItem
    {
        /// <summary>
        /// Armor class.
        /// </summary>
        double ArmorClass { get; set; }

        /// <summary>
        /// List of areas protected by the armor.
        /// </summary>
        string[] ArmoredAreas { get; set; }

        /// <summary>
        /// Durability
        /// </summary>
        double Durability { get; set; }

        /// <summary>
        /// Modifier added to the weapon ergonomics in percentage.
        /// </summary>
        double ErgonomicsPercentageModifier { get; set; }

        /// <summary>
        /// Material which composes the armor.
        /// </summary>
        string Material { get; set; }

        /// <summary>
        /// Modifier added to the character movement speed in percentage.
        /// </summary>
        double MovementSpeedPercentageModifier { get; set; }

        /// <summary>
        /// Chance of ricochet.
        /// </summary>
        string? RicochetChance { get; set; }

        /// <summary>
        /// Modifier added to the character turning speed in percentage.
        /// </summary>
        double TurningSpeedPercentageModifier { get; set; }
    }
}
