using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an Azure blob storage fetcher.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BlobFetcher : IBlobFetcher
    {
        /// <summary>
        /// Azure Functions configuration cache.
        /// </summary>
        private readonly IAzureFunctionsConfigurationCache AzureFunctionsConfigurationCache;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<BlobFetcher> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureFunctionsConfigurationCache">Azure Functions configuration cache.</param>
        public BlobFetcher(ILogger<BlobFetcher> logger, IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache)
        {
            AzureFunctionsConfigurationCache = azureFunctionsConfigurationCache;
            Logger = logger;
        }

        /// <inheritdoc/>
        public Task<Result<string>> Fetch(string blobName)
        {
            return Task.Run(() => ExecuteFetch(blobName));
        }

        /// <summary>
        /// Fetches the value of an Azure Blob storage.
        /// </summary>
        /// <param name="blobName">Name of the blob.</param>
        /// <returns>Blob value.</returns>
        private Result<string> ExecuteFetch(string blobName)
        {
            if (string.IsNullOrWhiteSpace(AzureFunctionsConfigurationCache.Values.AzureBlobStorageConnectionString)
                || string.IsNullOrWhiteSpace(AzureFunctionsConfigurationCache.Values.AzureBlobStorageContainerName))
            {
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
            }

            string blobData;

            try
            {
                BlobContainerClient blobContainerClient = new(AzureFunctionsConfigurationCache.Values.AzureBlobStorageConnectionString, AzureFunctionsConfigurationCache.Values.AzureBlobStorageContainerName);
                BlockBlobClient blockBlobClient = blobContainerClient.GetBlockBlobClient(blobName);

                using MemoryStream memoryStream = new();
                Task fetchTask = blockBlobClient.DownloadToAsync(memoryStream);

                if (!fetchTask.Wait(AzureFunctionsConfigurationCache.Values.FetchTimeout * 1000))
                {
                    string error = Properties.Resources.FetchingDelayExceeded;
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                memoryStream.Flush();
                memoryStream.Position = 0;
                StreamReader streamReader = new(memoryStream);
                blobData = streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.BlobFetchingError, blobName, e);
                Logger.LogError(error);

                return Result.Fail(error);
            }

            return Result.Ok(blobData);
        }
    }
}
