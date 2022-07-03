namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of an item.
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// Caption.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// ID of the category of the item.
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// IDs of conflicting items.
        /// </summary>
        public string[] ConflictingItemIds { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether the item has market data or not.
        /// </summary>
        public bool HasMarketData { get; set; }

        /// <summary>
        /// Link to the icon.
        /// </summary>
        public string IconLink { get; set; }

        /// <summary>
        /// ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Link to the image.
        /// </summary>
        public string ImageLink { get; set; }

        /// <summary>
        /// Maximum number of times the item can be stacked.
        /// </summary>
        public double MaxStackableAmount { get; set; }

        /// <summary>
        /// Link to the item market page.
        /// </summary>
        public string MarketLink { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Prices.
        /// </summary>
        public IPrice[] Prices { get; set; }

        /// <summary>
        /// Short name.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Weight in kilograms.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Link to the item wiki page.
        /// </summary>
        public string WikiLink { get; set; }
    }
}
