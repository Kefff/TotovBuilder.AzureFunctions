using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a quests fetcher.
    /// </summary>
    public interface IQuestsFetcher : IApiFetcher<IEnumerable<Quest>>
    {
    }
}
