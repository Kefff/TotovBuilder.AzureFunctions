using System;
using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a magazine.
    /// </summary>
    public class Magazine : Item, IMagazine
    {
        /// <inheritdoc/>
        public string[] AcceptedAmmunitionIds { get; set; } = Array.Empty<string>();

        /// <inheritdoc/>
        public double CheckSpeedPercentageModifier { get; set; }

        /// <inheritdoc/>
        public double LoadSpeedPercentageModifier { get; set; }

        /// <inheritdoc/>
        public double Capacity { get; set; }

        /// <inheritdoc/>
        public double ErgonomicsModifier { get; set; }

        /// <inheritdoc/>
        public IModSlot[] ModSlots { get; set; } = Array.Empty<IModSlot>();
    }
}
