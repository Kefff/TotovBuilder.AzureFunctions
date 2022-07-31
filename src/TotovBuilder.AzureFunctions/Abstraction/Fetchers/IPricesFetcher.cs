using System.Collections.Generic;
using TotovBuilder.AzureFunctions.Models.Items;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a prices fetcher.
    /// </summary>
    public interface IPricesFetcher : IApiFetcher<IEnumerable<Item>>
    {
    }
}
