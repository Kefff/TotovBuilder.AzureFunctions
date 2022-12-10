﻿using System.Diagnostics.CodeAnalysis;
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
        /// Azure Functions configuration wrapper.
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
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration wrapper.</param>
        public BlobFetcher(ILogger<BlobFetcher> logger, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
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
            if (string.IsNullOrWhiteSpace(AzureFunctionsConfigurationReader.Values.AzureBlobStorageConnectionString)
                || string.IsNullOrWhiteSpace(AzureFunctionsConfigurationReader.Values.AzureBlobStorageContainerName))
            {
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
            }

            string blobData;

            try
            {
                BlobContainerClient blobContainerClient = new(AzureFunctionsConfigurationReader.Values.AzureBlobStorageConnectionString, AzureFunctionsConfigurationReader.Values.AzureBlobStorageContainerName);
                BlockBlobClient blockBlobClient = blobContainerClient.GetBlockBlobClient(blobName);

                using MemoryStream memoryStream = new();
                Task fetchTask = blockBlobClient.DownloadToAsync(memoryStream);

                if (!fetchTask.Wait(AzureFunctionsConfigurationReader.Values.FetchTimeout * 1000))
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
