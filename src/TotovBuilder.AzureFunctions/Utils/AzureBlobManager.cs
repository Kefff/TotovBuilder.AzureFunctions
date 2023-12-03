using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Utils;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents an Azure blob manager.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AzureBlobManager : IAzureBlobManager
    {
        /// <summary>
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<AzureBlobManager> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobManager"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public AzureBlobManager(ILogger<AzureBlobManager> logger, IConfigurationWrapper configurationWrapper)
        {
            ConfigurationWrapper = configurationWrapper;
            Logger = logger;
        }

        /// <inheritdoc/>
        public Task<Result<string>> Fetch(string azureBlobContainerName, string azureBlobName)
        {
            return Task.Run(() => ExecuteFetch(azureBlobContainerName, azureBlobName));
        }

        /// <inheritdoc/>
        public Task Update(string azureBlobContainerName, string azureBlobName, object data)
        {
            // TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetches the value of an Azure blob storage.
        /// </summary>
        /// <param name="azureBlobContainerName">Name of the Azure blob container that contains the blob.</param>
        /// <param name="azureBlobName">Name of the blob.</param>
        /// <returns>Blob value.</returns>
        private Result<string> ExecuteFetch(string azureBlobContainerName, string azureBlobName)
        {
            if (string.IsNullOrWhiteSpace(ConfigurationWrapper.Values.AzureBlobStorageConnectionString)
                || string.IsNullOrWhiteSpace(ConfigurationWrapper.Values.AzureBlobStorageRawDataContainerName))
            {
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
            }

            string blobData;

            try
            {
                BlobContainerClient blobContainerClient = new BlobContainerClient(ConfigurationWrapper.Values.AzureBlobStorageConnectionString, azureBlobContainerName);
                BlockBlobClient blockBlobClient = blobContainerClient.GetBlockBlobClient(azureBlobName);

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
                string error = string.Format(Properties.Resources.AzureBlobFetchingError, azureBlobName, e);
                Logger.LogError(error);

                return Result.Fail(error);
            }

            return Result.Ok(blobData);
        }
    }
}
