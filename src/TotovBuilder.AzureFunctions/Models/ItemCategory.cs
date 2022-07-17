using System;

namespace TotovBuilder.AzureFunctions.Models
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
        public TarkovItemCategory[] TarkovItemCategories = Array.Empty<TarkovItemCategory>();
    }

    /// <summary>
    /// Represents an item category in Tarkov.
    /// </summary>
    public class TarkovItemCategory
    {
        /// <summary>
        /// ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
