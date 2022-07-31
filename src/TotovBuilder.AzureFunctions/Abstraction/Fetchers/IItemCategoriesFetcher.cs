using System.Collections.Generic;
using TotovBuilder.AzureFunctions.Models.Items;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of an item categories fetcher.
    /// </summary>
    public interface IItemCategoriesFetcher : IApiFetcher<IEnumerable<ItemCategory>>
    {
    }
}
