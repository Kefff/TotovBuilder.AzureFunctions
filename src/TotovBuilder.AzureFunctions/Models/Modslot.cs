using System;
using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a mod slot.
    /// </summary>
    public class Modslot : IModSlot
    {
        /// <inheritdoc/>
        public string[] CompatibleItemIds { get; set; } = Array.Empty<string>();
        
        /// <inheritdoc/>
        public string Id { get; set; } = string.Empty;

        /// <inheritdoc/>
        public double MaxStackableAmount { get; set; } = 1;

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;
        
        /// <inheritdoc/>
        public bool Required { get; set; }
    }
}
