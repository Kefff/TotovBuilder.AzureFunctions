using FluentResults;

namespace TotovBuilder.AzureFunctions.Abstractions.Utils
{
    /// <summary>
    /// Provides the functionalities of an Azure blob manager.
    /// </summary>
    public interface IAzureBlobManager
    {
        /// <summary>
        /// Fetches data from an Azure blob.
        /// </summary>
        /// <param name="azureBlobName">Name of the blob.</param>
        /// <returns>Blob data.</returns>
        Task<Result<string>> Fetch(string azureBlobName);

        /// <summary>
        /// Updates data of an Azure blob.
        /// </summary>
        /// <param name="azureBlobName">Name of the blob.</param>
        /// <param name="data">Data to upload.</param>
        Task<Result> Update(string azureBlobName, object data);
    }
}
