using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
        public Task<Result<string>> Fetch(string azureBlobName)
        {
            return Task.Run(() => ExecuteFetch(azureBlobName));
        }

        /// <inheritdoc/>
        public Task<Result> Update(string azureBlobName, object data)
        {
            return Task.Run(() => ExecuteUpdate(azureBlobName, data));
        }

        /// <summary>
        /// Fetches the value of an Azure blob.
        /// </summary>
        /// <param name="azureBlobName">Name of the blob.</param>
        /// <returns>Blob value.</returns>
        private Result<string> ExecuteFetch(string azureBlobName)
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
                BlobContainerClient blobContainerClient = new BlobContainerClient(ConfigurationWrapper.Values.AzureBlobStorageConnectionString, ConfigurationWrapper.Values.AzureBlobStorageRawDataContainerName);
                BlockBlobClient blockBlobClient = blobContainerClient.GetBlockBlobClient(azureBlobName);

                using MemoryStream memoryStream = new MemoryStream();
                Task fetchTask = blockBlobClient.DownloadToAsync(memoryStream);

                if (!fetchTask.Wait(ConfigurationWrapper.Values.ExecutionTimeout * 1000))
                {
                    string error = Properties.Resources.ExecutionDelayExceeded;
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

        /// <summary>
        /// Updates an Azure blob.
        /// </summary>
        /// <param name="azureBlobName">Name of the blob.</param>
        /// <param name="data">Data to upload.</param>
        private Result ExecuteUpdate(string azureBlobName, object data)
        {
            if (string.IsNullOrWhiteSpace(ConfigurationWrapper.Values.AzureBlobStorageConnectionString)
                || string.IsNullOrWhiteSpace(ConfigurationWrapper.Values.AzureBlobStorageWebsiteDataContainerName))
            {
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
            }

            try
            {
                BlobContainerClient blobContainerClient = new BlobContainerClient(ConfigurationWrapper.Values.AzureBlobStorageConnectionString, ConfigurationWrapper.Values.AzureBlobStorageWebsiteDataContainerName);
                BlockBlobClient blockBlobClient = blobContainerClient.GetBlockBlobClient(azureBlobName);
                BlobHttpHeaders httpHeaders = new BlobHttpHeaders { ContentType = MimeMapping.MimeUtility.GetMimeMapping(azureBlobName) };

                using MemoryStream memoryStream = new MemoryStream();
                StreamWriter writer = new StreamWriter(memoryStream);
                writer.Write(data);
                writer.Flush();
                memoryStream.Position = 0;

                Task updateTask = blockBlobClient.UploadAsync(memoryStream, httpHeaders);

                if (!updateTask.Wait(ConfigurationWrapper.Values.ExecutionTimeout * 1000))
                {
                    string error = Properties.Resources.ExecutionDelayExceeded;
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                return Result.Ok();
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.AzureBlobUpdatingError, azureBlobName, e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }
    }
}
