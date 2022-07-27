namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a mod slot in an inventory item.
    /// </summary>
    public class InventoryModSlot
    {
        /// <summary>
        /// Item.
        /// </summary>
        public InventoryItem? Item { get; set; }

        /// <summary>
        /// Name of the mod slot.
        /// </summary>
        public string ModSlotName { get; set; } = string.Empty;
    }
}
