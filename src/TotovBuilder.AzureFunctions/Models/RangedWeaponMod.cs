using System;
using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a ranged weapon mod.
    /// </summary>
    public class RangedWeaponMod : Item, IRangedWeaponMod
    {

        /// <inheritdoc/>
        public double AccuracyPercentageModifier { get; set; }

        /// <inheritdoc/>
        public double RecoilPercentageModifier { get; set; }

        /// <inheritdoc/>
        public double ErgonomicsModifier { get; set; }

        /// <inheritdoc/>
        public ModSlot[] ModSlots { get; set; } = Array.Empty<ModSlot>();
    }
}
