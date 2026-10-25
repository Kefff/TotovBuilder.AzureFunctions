using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a static data fetcher.
    /// </summary>
    public abstract class RawDataFetcher<T> : ApiFetcher<T>
        where T : class
    {
        /// <summary>
        /// Serialization options.
        /// </summary>
        protected static readonly JsonSerializerOptions SerializationOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Configuration wrapper;
        /// </summary>
        protected readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Name of the Azure blob that stores the data.
        /// </summary>
        protected abstract string AzureBlobName { get; }

        /// <summary>
        /// Azure blob storage manager.
        /// </summary>
        private readonly IAzureBlobStorageManager AzureBlobStorageManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawDataFetcher{T}"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureBlobStorageManager">Azure blob storage manager.</param>
        /// <param name="configurationWrapper">Configuration wrapper</param>
        protected RawDataFetcher(
            ILogger<RawDataFetcher<T>> logger,
            IAzureBlobStorageManager azureBlobStorageManager,
            IConfigurationWrapper configurationWrapper)
            : base(logger)
        {
            AzureBlobStorageManager = azureBlobStorageManager;
            ConfigurationWrapper = configurationWrapper;
        }

        /// <summary>
        /// Deserializes data from a the content of a fetch response.
        /// </summary>
        /// <param name="responseContent">Content of a fetch response.</param>
        /// <returns>Deserialized data.</returns>
        protected abstract Task<Result<T>> DeserializeData(string responseContent);

        /// <inheritdoc/>
        protected override async Task<Result<T>> ExecuteFetch()
        {
            Result<string> blobFetchResult = await AzureBlobStorageManager.FetchBlob(ConfigurationWrapper.Values.AzureBlobStorageRawDataContainerName, AzureBlobName);

            if (!blobFetchResult.IsSuccess)
            {
                return blobFetchResult.ToResult<T>();
            }

            Result<T> deserializedData = await DeserializeData(blobFetchResult.Value);

            return deserializedData;
        }
    }
}
