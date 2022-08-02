using System.Threading.Tasks;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a base class for API fetchers.
    /// </summary>
    public interface IApiFetcher<T>
        where T: class
    {
        /// <summary>
        /// Fetches barter data from the API.
        /// </summary>
        /// <returns>Fetched data.</returns>
        Task<T?> Fetch();
    }
}
