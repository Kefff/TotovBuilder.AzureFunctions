namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a base class for API fetchers.
    /// </summary>
    public interface IApiFetcher<T>
        where T : class
    {
        /// <summary>
        /// Fetches data from the API.
        /// </summary>
        /// <remarks>Can return null because we cannot know what the "default" value should be when the API call fails.</remarks>
        /// <returns>Data fetched as a JSON string.</returns>
        Task<T> Fetch();
    }
}
