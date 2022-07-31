using System;

namespace TotovBuilder.AzureFunctions.Models.Builds
{
    /// <summary>
    /// Represents an item in the inventory.
    /// </summary>
    public class InventoryItem
    {
        /// <summary>
        /// Items contained in the item.
        /// </summary>
        public InventoryItem[] Content { get; set; } = Array.Empty<InventoryItem>();

        /// <summary>
        /// Indicates whether the price of the item is used or not.
        /// When undefined, the price is taken in consideration.
        /// </summary>
        public bool IgnorePrice { get; set; } = false;

        /// <summary>
        /// Item ID.
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// Mods slots.
        /// </summary>
        public InventoryModSlot[] ModSlots = Array.Empty<InventoryModSlot>();

        /// <summary>
        /// Number of items stacked.
        /// </summary>
        public double Quantity { get; set; } = 1;
    }
}
