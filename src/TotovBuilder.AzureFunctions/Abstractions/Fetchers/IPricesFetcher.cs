using System.Collections.Generic;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a prices fetcher.
    /// </summary>
    public interface IPricesFetcher : IApiFetcher<IEnumerable<Item>>
    {
    }
}
