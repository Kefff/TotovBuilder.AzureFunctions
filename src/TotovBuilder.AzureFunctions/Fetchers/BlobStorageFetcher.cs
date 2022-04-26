using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an Azure blob storage fetcher.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BlobStorageFetcher : IBlobDataFetcher
    {
        /// <summary>
        /// Azure blob store connection string.
        /// </summary>
        private readonly string AzureBlobStorageConnectionString;

        /// <summary>
        /// Azure blob storage container name.
        /// </summary>
        private readonly string AzureBlobStorageContainerName;

        /// <summary>
        /// Fetch timeout in seconds.
        /// </summary>
        private readonly int FetchTimeout;

        /// <summary>
        /// Configuration reader.
        /// </summary>
        private readonly IConfigurationReader ConfigurationReader;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<BlobStorageFetcher> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        public BlobStorageFetcher(ILogger<BlobStorageFetcher> logger, IConfigurationReader configurationReader)
        {
            ConfigurationReader = configurationReader;
            Logger = logger;

            AzureBlobStorageConnectionString = ConfigurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.AzureBlobStorageConnectionStringKey);
            AzureBlobStorageContainerName = ConfigurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.AzureBlobStorageContainerNameKey);
            FetchTimeout = ConfigurationReader.ReadInt(TotovBuilder.AzureFunctions.ConfigurationReader.FetchTimeoutKey);
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
            if (string.IsNullOrWhiteSpace(AzureBlobStorageConnectionString)
                || string.IsNullOrWhiteSpace(AzureBlobStorageContainerName))
            {
                return Result.Fail(string.Empty);
            }

            string blobData;

            try
            {
                CloudStorageAccount cloudBlobAccount = CloudStorageAccount.Parse(AzureBlobStorageConnectionString);
                CloudBlobClient cloudBlobClient = cloudBlobAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(AzureBlobStorageContainerName);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);

                using MemoryStream memoryStream = new MemoryStream();
                Task fetchTask = cloudBlockBlob.DownloadToStreamAsync(memoryStream);

                if (!fetchTask.Wait(FetchTimeout * 1000))
                {
                    Logger.LogError(Properties.Resources.FetchingDelayExceeded);

                    return Result.Fail(string.Empty);
                }

                memoryStream.Flush();
                memoryStream.Position = 0;
                StreamReader streamReader = new StreamReader(memoryStream);
                blobData = streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Logger.LogError(Properties.Resources.BlobFetchingError, blobName, e);

                return Result.Fail(string.Empty);
            }

            return Result.Ok(blobData);
        }
    }
}
