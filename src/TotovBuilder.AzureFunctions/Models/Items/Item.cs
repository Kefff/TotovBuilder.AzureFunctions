using System;
using TotovBuilder.AzureFunctions.Abstraction.Models.Items;

namespace TotovBuilder.AzureFunctions.Models.Items
{
    /// <summary>
    /// Represents an item.
    /// </summary>
    public class Item : IItem
    {
        /// <inheritdoc/>
        public string CategoryId { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string[] ConflictingItemIds { get; set; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string IconLink { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string Id { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string ImageLink { get; set; } = string.Empty;

        /// <inheritdoc/>
        public double MaxStackableAmount { get; set; } = 1;

        /// <inheritdoc/>
        public string MarketLink { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public Price[] Prices { get; set; } = Array.Empty<Price>();

        /// <inheritdoc/>
        public string ShortName { get; set; } = string.Empty;

        /// <inheritdoc/>
        public double Weight { get; set; }

        /// <inheritdoc/>
        public string WikiLink { get; set; } = string.Empty;
    }
}
