using TotovBuilder.Model.Builds;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a presets fetcher.
    /// </summary>
    public interface IPresetsFetcher : IApiFetcher<IEnumerable<InventoryItem>>
    {
    }
}
