using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a changelog fetcher.
    /// </summary>
    public interface IChangelogFetcher : IApiFetcher<IEnumerable<ChangelogEntry>>
    {
    }
}
