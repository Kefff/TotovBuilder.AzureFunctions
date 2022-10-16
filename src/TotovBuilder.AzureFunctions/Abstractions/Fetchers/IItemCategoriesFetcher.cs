using System.Collections.Generic;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of an item categories fetcher.
    /// </summary>
    public interface IItemCategoriesFetcher : IApiFetcher<IEnumerable<ItemCategory>>
    {
    }
}
