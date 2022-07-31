using System.Collections.Generic;
using TotovBuilder.AzureFunctions.Models.Builds;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a presets fetcher.
    /// </summary>
    public interface IPresetsFetcher : IApiFetcher<IEnumerable<InventoryItem>>
    {
    }
}
