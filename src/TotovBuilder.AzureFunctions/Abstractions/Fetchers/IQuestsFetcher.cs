using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a quests fetcher.
    /// </summary>
    public interface IQuestsFetcher : IApiFetcher<IReadOnlyDictionary<string, Quest>>
    {
        /// <summary>
        /// Gets a fetched quest.
        /// </summary>
        /// <param name="questId">Quest ID.</param>
        /// <returns>Quest.</returns>
        /// <exception cref="InvalidDataException">Thrown when the quest has not been fetched.</exception>
        Quest GetQuest(string questId);
    }
}
