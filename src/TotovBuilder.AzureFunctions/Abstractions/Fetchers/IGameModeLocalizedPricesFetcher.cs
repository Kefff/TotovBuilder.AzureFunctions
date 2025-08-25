using FluentResults;
using TotovBuilder.Model.Utils;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a fetcher that fetches localized prices for a game mode.
    /// </summary>
    public interface IGameModeLocalizedPricesFetcher
    {
        /// <summary>
        /// Fetched data.
        /// Once data has been fetched and stored in this property, it is never fetched again.
        /// </summary>
        IEnumerable<GameModeLocalizedPrices>? FetchedData { get; }

        /// <summary>
        /// Fetches data from the API.
        /// </summary>
        /// <returns>Data fetch result.</returns>
        Task<Result> Fetch();
    }
}
