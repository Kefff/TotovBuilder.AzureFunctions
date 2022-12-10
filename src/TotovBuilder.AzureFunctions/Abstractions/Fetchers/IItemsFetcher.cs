using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of an items fetcher.
    /// </summary>
    public interface IItemsFetcher : IApiFetcher<IEnumerable<Item>>
    {
    }
}
