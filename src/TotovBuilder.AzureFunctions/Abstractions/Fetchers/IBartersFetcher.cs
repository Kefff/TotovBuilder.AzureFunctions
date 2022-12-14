using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a barters fetcher.
    /// </summary>
    public interface IBartersFetcher : IApiFetcher<IEnumerable<Price>>
    {
    }
}
