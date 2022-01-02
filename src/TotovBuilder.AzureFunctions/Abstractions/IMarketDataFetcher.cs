using FluentResults;
using System.Threading.Tasks;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a market data fetcher.
    /// </summary>
    public interface IMarketDataFetcher
    {
        /// <summary>
        /// Fetches market data from the API.
        /// </summary>
        /// <returns>Queried data.</returns>
        Task<Result<string>> Fetch();
    }
}