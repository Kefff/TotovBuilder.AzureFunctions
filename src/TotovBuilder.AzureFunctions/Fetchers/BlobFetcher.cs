using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
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
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<BlobFetcher> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public BlobFetcher(ILogger<BlobFetcher> logger, IConfigurationWrapper configurationWrapper)
        {
            ConfigurationWrapper = configurationWrapper;
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
            if (string.IsNullOrWhiteSpace(ConfigurationWrapper.Values.AzureBlobStorageConnectionString)
                || string.IsNullOrWhiteSpace(ConfigurationWrapper.Values.AzureBlobStorageContainerName))
            {
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
            }

            string blobData;

            try
            {
                BlobContainerClient blobContainerClient = new BlobContainerClient(ConfigurationWrapper.Values.AzureBlobStorageConnectionString, ConfigurationWrapper.Values.AzureBlobStorageContainerName);
                BlockBlobClient blockBlobClient = blobContainerClient.GetBlockBlobClient(blobName);

                using MemoryStream memoryStream = new MemoryStream();
                Task fetchTask = blockBlobClient.DownloadToAsync(memoryStream);

                if (!fetchTask.Wait(ConfigurationWrapper.Values.FetchTimeout * 1000))
                {
                    string error = Properties.Resources.FetchingDelayExceeded;
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                memoryStream.Flush();
                memoryStream.Position = 0;
                StreamReader streamReader = new StreamReader(memoryStream);
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
