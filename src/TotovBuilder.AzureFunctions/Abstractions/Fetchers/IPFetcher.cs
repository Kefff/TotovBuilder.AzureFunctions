using TotovBuilder.Model.Builds;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a presets fetcher.
    /// </summary>
    public interface IPFetcher : IApiFetcher<IEnumerable<InventoryItem>>
    {
    }
}
