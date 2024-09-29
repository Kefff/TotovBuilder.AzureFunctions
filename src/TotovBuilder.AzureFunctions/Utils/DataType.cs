namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Determines the available data types.
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Totov Builder Azure Functions configuration.
        /// </summary>
        AzureFunctionsConfiguration,

        /// <summary>
        /// Item categories.
        /// </summary>
        Barters,

        /// <summary>
        /// Changelog.
        /// </summary>
        Changelog,

        /// <summary>
        /// Item categories.
        /// </summary>
        ItemCategories,

        /// <summary>
        /// Item missing properties.
        /// </summary>
        ItemMissingProperties,

        /// <summary>
        /// Items.
        /// </summary>
        Items,

        /// <summary>
        /// Presets.
        /// </summary>
        Presets,

        /// <summary>
        /// Prices.
        /// </summary>
        Prices,

        /// <summary>
        /// Quests.
        /// </summary>
        Quests,

        /// <summary>
        /// Values related to Tarkov gameplay.
        /// </summary>
        TarkovValues,

        /// <summary>
        /// Totov Builder website configuration.
        /// </summary>
        WebsiteConfiguration
    }
}
