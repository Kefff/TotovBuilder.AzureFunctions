using FluentResults;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Utils;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a fetcher that fetches localized prices for a game mode.
    /// </summary>
    public class GameModeLocalizedPricesFetcher : IGameModeLocalizedPricesFetcher
    {
        /// <inheritdoc/>
        public IEnumerable<GameModeLocalizedPrices>? FetchedData
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
