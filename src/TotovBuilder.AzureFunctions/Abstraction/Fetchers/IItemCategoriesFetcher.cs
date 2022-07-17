using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of an item categories fetcher.
    /// </summary>
    public interface IItemCategoriesFetcher : IApiFetcher<ItemCategory[]>
    {
    }
}
