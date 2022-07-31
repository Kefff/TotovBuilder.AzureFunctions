using System;

namespace TotovBuilder.AzureFunctions.Models.Items
{
    /// <summary>
    /// Represents an item category.
    /// </summary>
    public class ItemCategory
    {
        /// <summary>
        /// ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Tarkov item categories included in this item category.
        /// </summary>
        public ItemType[] Types { get; set; } = Array.Empty<ItemType>();
    }
}
