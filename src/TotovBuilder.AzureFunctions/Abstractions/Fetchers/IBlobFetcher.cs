using System.Threading.Tasks;
using FluentResults;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a blob fetcher.
    /// </summary>
    public interface IBlobFetcher
    {
        /// <summary>
        /// Fetches the value of an Azure Blob storage.
        /// </summary>
        /// <param name="blobName">Name of the blob.</param>
        /// <returns>Blob value.</returns>
        Task<Result<string>> Fetch(string blobName);
    }
}
