using System;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a mod slot.
    /// </summary>
    public class ModSlot
    {
        /// <summary>
        /// IDs of compatible items.
        /// </summary>
        public string[] CompatibleItemIds { get; set; } = Array.Empty<string>();

        /// <summary>
        /// ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of times the item can be stacked in this mod slot.
        /// Mainly used to force the ammunition quantity to 1 in the special chamber mod slot.
        /// </summary>
        public double MaxStackableAmount { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Determines whether having an item in the mod slot is required for the parent item to be usable.
        /// </summary>
        public bool Required { get; set; }
    }
}
