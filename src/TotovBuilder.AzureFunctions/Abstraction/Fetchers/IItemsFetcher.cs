using System.Collections.Generic;
using TotovBuilder.AzureFunctions.Models.Items;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of an items fetcher.
    /// </summary>
    public interface IItemsFetcher : IApiFetcher<IEnumerable<Item>>
    {
    }
}
