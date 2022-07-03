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
        public double ArmorClass { get; set; }

        /// <summary>
        /// List of areas protected by the armor.
        /// </summary>
        public string[] ArmoredAreas { get; set; }

        /// <summary>
        /// Durability
        /// </summary>
        public double Durability { get; set; }

        /// <summary>
        /// Modifier added to the weapon ergonomics in percentage.
        /// </summary>
        public double ErgonomicsPercentageModifier { get; set; }

        /// <summary>
        /// Material which composes the armor.
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// Modifier added to the character movement speed in percentage.
        /// </summary>
        public double MovementSpeedPercentageModifier { get; set; }

        /// <summary>
        /// Chance of ricochet.
        /// </summary>
        public string? RicochetChance { get; set; }

        /// <summary>
        /// Modifier added to the character turning speed in percentage.
        /// </summary>
        public double TurningSpeedPercentageModifier { get; set; }
    }
}
