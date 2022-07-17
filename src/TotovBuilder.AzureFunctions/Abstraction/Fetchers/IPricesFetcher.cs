using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a prices fetcher.
    /// </summary>
    public interface IPricesFetcher : IApiFetcher<Price[]>
    {
    }
}
