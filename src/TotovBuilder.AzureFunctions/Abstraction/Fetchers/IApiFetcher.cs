using System.Threading.Tasks;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a base class for API fetchers.
    /// </summary>
    public interface IApiFetcher
    {
        /// <summary>
        /// Fetches barter data from the API.
        /// </summary>
        /// <returns>Fetched data.</returns>
        Task<string> Fetch();
    }
}
