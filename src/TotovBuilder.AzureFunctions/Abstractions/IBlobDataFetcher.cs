using System.Threading.Tasks;
using FluentResults;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a blob data fetcher.
    /// </summary>
    public interface IBlobDataFetcher
    {
        /// <summary>
        /// Fetches the value of an Azure Blob storage.
        /// </summary>
        /// <param name="blobName">Name of the blob.</param>
        /// <returns>Blob value.</returns>
        public Task<Result<string>> Fetch(string blobName);
    }
}
