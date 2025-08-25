using FluentResults;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Utils;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a fetcher that fetches localized items for a game mode.
    /// </summary>
    public class GameModeLocalizedItemsFetcher : IGameModeLocalizedItemsFetcher
    {
        /// <inheritdoc/>
        public IEnumerable<GameModeLocalizedItems>? FetchedData
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        /// <inheritdoc/>
        public Task<Result> Fetch()
        {
            throw new NotImplementedException();
        }
    }
}
