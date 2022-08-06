using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
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
        private readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<BlobFetcher> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        public BlobFetcher(ILogger<BlobFetcher> logger, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader ;
            Logger = logger;

            AzureBlobStorageConnectionString = AzureFunctionsConfigurationReader.Values.AzureBlobStorageConnectionString;
            AzureBlobStorageContainerName = AzureFunctionsConfigurationReader.Values.AzureBlobStorageContainerName;
            FetchTimeout = AzureFunctionsConfigurationReader.Values.FetchTimeout;
        }

        /// <inheritdoc/>
        public async Task<Result<string>> Fetch(string blobName)
        {
            await AzureFunctionsConfigurationReader.WaitForLoading(); // Awaiting for the configuration to be loaded

            return ExecuteFetch(blobName);
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
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
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
