using FluentResults;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a base class for API fetchers.
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
        /// Fetches data from the API.
        /// </summary>
        /// <remarks>Can return null because we cannot know what the "default" value should be when the API call fails.</remarks>
        /// <returns>Data fetch result.</returns>
        Task<Result> Fetch();
    }
}
