using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a Tarkov values fetcher.
    /// </summary>
    public interface ITarkovValuesFetcher : IApiFetcher<TarkovValues>
    {
    }
}
