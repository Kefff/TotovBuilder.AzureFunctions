using System;
using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a mod.
    /// </summary>
    public class Mod : Item, IMod
    {
        /// <inheritdoc/>
        public double ErgonomicsModifier { get; set; }

        /// <inheritdoc/>
        public IModSlot[] ModSlots { get; set; } = Array.Empty<IModSlot>();
    }
}
