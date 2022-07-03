using System;
using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
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
        public IModSlot[] ModSlots { get; set; } = Array.Empty<IModSlot>();
    }
}
