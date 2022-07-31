using System.Collections.Generic;
using TotovBuilder.AzureFunctions.Models.Items;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a barters fetcher.
    /// </summary>
    public interface IBartersFetcher : IApiFetcher<IEnumerable<Item>>
    {
    }
}
