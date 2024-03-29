﻿using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of an item missing properties fetcher.
    /// </summary>
    public interface IItemMissingPropertiesFetcher : IApiFetcher<IEnumerable<ItemMissingProperties>>
    {
    }
}
