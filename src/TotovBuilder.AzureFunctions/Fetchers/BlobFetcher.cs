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
        /// Azure Functions configuration wrapper.
        /// </summary>
        private readonly IAzureFunctionsConfigurationWrapper AzureFunctionsConfigurationWrapper;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<BlobFetcher> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureFunctionsConfigurationWrapper">Azure Functions configuration wrapper.</param>
        public BlobFetcher(ILogger<BlobFetcher> logger, IAzureFunctionsConfigurationWrapper azureFunctionsConfigurationWrapper)
        {
            AzureFunctionsConfigurationWrapper = azureFunctionsConfigurationWrapper;
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
            if (string.IsNullOrWhiteSpace(AzureFunctionsConfigurationWrapper.Values.AzureBlobStorageConnectionString)
                || string.IsNullOrWhiteSpace(AzureFunctionsConfigurationWrapper.Values.AzureBlobStorageContainerName))
            {
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
            }

            string blobData;

            try
            {
                CloudStorageAccount cloudBlobAccount = CloudStorageAccount.Parse(AzureFunctionsConfigurationWrapper.Values.AzureBlobStorageConnectionString);
                CloudBlobClient cloudBlobClient = cloudBlobAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(AzureFunctionsConfigurationWrapper.Values.AzureBlobStorageContainerName);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);

                using MemoryStream memoryStream = new MemoryStream();
                Task fetchTask = cloudBlockBlob.DownloadToStreamAsync(memoryStream);

                if (!fetchTask.Wait(AzureFunctionsConfigurationWrapper.Values.FetchTimeout * 1000))
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
