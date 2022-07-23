using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of an item.
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// ID of the category of the item.
        /// </summary>
        string CategoryId { get; set; }

        /// <summary>
        /// IDs of conflicting items.
        /// </summary>
        string[] ConflictingItemIds { get; set; }

        /// <summary>
        /// Link to the icon.
        /// </summary>
        string IconLink { get; set; }

        /// <summary>
        /// ID.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Link to the image.
        /// </summary>
        string ImageLink { get; set; }

        /// <summary>
        /// Maximum number of times the item can be stacked.
        /// </summary>
        double MaxStackableAmount { get; set; }

        /// <summary>
        /// Link to the item market page.
        /// </summary>
        string MarketLink { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Prices.
        /// </summary>
        Price[] Prices { get; set; }

        /// <summary>
        /// Short name.
        /// </summary>
        string ShortName { get; set; }

        /// <summary>
        /// Weight in kilograms.
        /// </summary>
        double Weight { get; set; }

        /// <summary>
        /// Link to the item wiki page.
        /// </summary>
        string WikiLink { get; set; }
    }
}
