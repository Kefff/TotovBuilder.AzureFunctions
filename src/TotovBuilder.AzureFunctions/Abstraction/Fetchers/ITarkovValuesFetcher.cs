using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a Tarkov values fetcher.
    /// </summary>
    public interface ITarkovValuesFetcher : IApiFetcher<TarkovValues>
    {
    }
}
