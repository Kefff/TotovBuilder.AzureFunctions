using System.Collections.Generic;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of an item missing properties fetcher.
    /// </summary>
    public interface IItemMissingPropertiesFetcher : IApiFetcher<IEnumerable<ItemMissingProperties>>
    {
    }
}
