using System.Collections.Generic;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a quests fetcher.
    /// </summary>
    public interface IQuestsFetcher : IApiFetcher<IEnumerable<Quest>>
    {
    }
}
