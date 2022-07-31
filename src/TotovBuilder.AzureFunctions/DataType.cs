﻿namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Determines the available data types.
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Azure Functions configuration.
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
        Quests
    }
}
