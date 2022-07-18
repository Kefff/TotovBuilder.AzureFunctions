using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a changelog fetcher.
    /// </summary>
    public interface IChangelogFetcher :IApiFetcher<ChangelogEntry[]>
    {
    }
}
