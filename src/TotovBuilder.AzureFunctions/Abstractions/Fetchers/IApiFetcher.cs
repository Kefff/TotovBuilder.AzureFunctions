using FluentResults;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a fetcher.
    /// </summary>
    public interface IApiFetcher<T>
        where T : class
    {
        /// <summary>
        /// Fetched data.
        /// Once data has been fetched and stored in this property, it is never fetched again.
        /// </summary>
        T? FetchedData { get; }

        /// <summary>
        /// Fetches data.
        /// </summary>
        /// <returns>Data fetch result.</returns>
        Task<Result> Fetch();
    }
}
