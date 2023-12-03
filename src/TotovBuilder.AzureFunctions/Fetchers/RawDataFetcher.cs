using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Utils;
using TotovBuilder.AzureFunctions.Utils;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a static data fetcher.
    /// </summary>
    public abstract class RawDataFetcher<T> : IApiFetcher<T>
        where T : class
    {
        /// <summary>
        /// Configuration wrapper;
        /// </summary>
        protected readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Name of the Azure blob that stores the data.
        /// </summary>
        protected abstract string AzureBlobName { get; }

        /// <summary>
        /// Type of data handled.
        /// </summary>
        protected abstract DataType DataType { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger<RawDataFetcher<T>> Logger;

        /// <summary>
        /// Azure blob manager.
        /// </summary>
        private readonly IAzureBlobManager AzureBlobManager;

        /// <summary>
        /// Fetched data.
        /// Once data has been fetched and stored in this property, it is never fetched again.
        /// </summary>
        private T? FetchedData { get; set; } = null;

        /// <summary>
        /// Fetching task.
        /// Used to avoid launching multiple fetch operations at the same time.
        /// </summary>
        private Task FetchingTask = Task.CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDataFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureBlobManager">Azure blob manager.</param>
        /// <param name="configurationWrapper">Configuration wrapper</param>
        protected RawDataFetcher(
            ILogger<RawDataFetcher<T>> logger,
            IAzureBlobManager azureBlobManager,
            IConfigurationWrapper configurationWrapper)
        {
            AzureBlobManager = azureBlobManager;
            ConfigurationWrapper = configurationWrapper;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Result<T>> Fetch()
        {
            if (!FetchingTask.IsCompleted)
            {
                Logger.LogInformation(Properties.Resources.StartWaitingForPreviousFetching, DataType.ToString());
                await FetchingTask;
                Logger.LogInformation(Properties.Resources.EndWaitingForPreviousFetching, DataType.ToString());
            }
            else
            {
                FetchingTask = Task.Run(async () =>
                {
                    if (FetchedData != null)
                    {
                        return;
                    }

                    Result<T> fetchResult = await ExecuteFetch();

                    if (fetchResult.IsSuccess)
                    {
                        FetchedData = fetchResult.Value;
                    }
                });
                await FetchingTask;
            }

            if (FetchedData == null)
            {
                return Result.Fail(string.Format(Properties.Resources.NoDataFetched, DataType.ToString()));
            }

            return Result.Ok(FetchedData);
        }

        /// <summary>
        /// Deserializes data from a the content of a fetch response.
        /// </summary>
        /// <param name="responseContent">Content of a fetch response.</param>
        /// <returns>Deserialized data.</returns>
        protected abstract Task<Result<T>> DeserializeData(string responseContent);

        /// <summary>
        /// Executes the fetch operation.
        /// </summary>
        /// <returns>Fetched data.</returns>
        private async Task<Result<T>> ExecuteFetch()
        {
            Logger.LogInformation(Properties.Resources.StartFetching, DataType.ToString());

            Result<string> blobFetchResult = await AzureBlobManager.Fetch(ConfigurationWrapper.Values.AzureBlobStorageRawDataContainerName, AzureBlobName);

            if (!blobFetchResult.IsSuccess)
            {
                return blobFetchResult.ToResult<T>();
            }

            Result<T> deserializedData = await DeserializeData(blobFetchResult.Value);

            Logger.LogInformation(Properties.Resources.EndFetching, DataType.ToString());

            return deserializedData;
        }
    }
}
