using System;
using TotovBuilder.AzureFunctions.Abstraction.Models.Items;

namespace TotovBuilder.AzureFunctions.Models.Items
{
    /// <summary>
    /// Represents headwear.
    /// </summary>
    public class Headwear : Item, IHeadwear
    {
        /// <inheritdoc/>
        public double ArmorClass { get; set; }

        /// <inheritdoc/>
        public string[] ArmoredAreas { get; set; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Deafening { get; set; } = string.Empty;

        /// <inheritdoc/>
        public double Durability { get; set; }

        /// <inheritdoc/>
        public double ErgonomicsPercentageModifier { get; set; }

        /// <inheritdoc/>
        public string Material { get; set; } = string.Empty;

        /// <inheritdoc/>
        public double MovementSpeedPercentageModifier { get; set; }

        /// <inheritdoc/>
        public string? RicochetChance { get; set; }

        /// <inheritdoc/>
        public double TurningSpeedPercentageModifier { get; set; }

        /// <inheritdoc/>
        public ModSlot[] ModSlots { get; set; } = Array.Empty<ModSlot>();
    }
}
