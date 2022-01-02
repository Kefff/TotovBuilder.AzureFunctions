using System.Threading.Tasks;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a data fetcher.
    /// </summary>
    public interface IDataFetcher
    {
        /// <summary>
        /// Fetches data.
        /// </summary>
        /// <param name="dataType">Data type.</param>
        /// <returns>Data as a JSON string.</returns>
        Task<string> Fetch(DataType dataType);
    }
}