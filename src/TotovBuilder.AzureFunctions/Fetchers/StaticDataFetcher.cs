using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a static data fetcher.
    /// </summary>
    public abstract class StaticDataFetcher<T> : IApiFetcher<T>
        where T : class
    {
        /// <summary>
        /// Azure Functions configuration cache;
        /// </summary>
        protected readonly IAzureFunctionsConfigurationCache AzureFunctionsConfigurationCache;

        /// <summary>
        /// Name of the Azure Blob that stores the data.
        /// </summary>
        protected abstract string AzureBlobName { get; }

        /// <summary>
        /// Type of data handled.
        /// </summary>
        protected abstract DataType DataType { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger<StaticDataFetcher<T>> Logger;

        /// <summary>
        /// Blob fetcher.
        /// </summary>
        private readonly IBlobFetcher BlobFetcher;

        /// <summary>
        /// Cache.
        /// </summary>
        private readonly ICache Cache;

        /// <summary>
        /// Fetching task.
        /// Used to avoid launching multiple fetch operations at the same time.
        /// </summary>
        private Task FetchingTask = Task.CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDataFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobFetcher">Blob fetcher.</param>
        /// <param name="azureFunctionsConfigurationCache">Azure Functions configuration cache</param>
        /// <param name="cache">Cache.</param>
        protected StaticDataFetcher(
            ILogger<StaticDataFetcher<T>> logger,
            IBlobFetcher blobFetcher,
            IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache,
            ICache cache)
        {
            BlobFetcher = blobFetcher;
            Cache = cache;
            AzureFunctionsConfigurationCache = azureFunctionsConfigurationCache;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<T?> Fetch()
        {
            if (!FetchingTask.IsCompleted)
            {
                Logger.LogInformation(Properties.Resources.StartWaitingForPreviousFetching, DataType.ToString());

                await FetchingTask;

                Logger.LogInformation(Properties.Resources.EndWaitingForPreviousFetching, DataType.ToString());

                return Cache.Get<T>(DataType);
            }

            T? result = null;

            FetchingTask = Task.Run(async () =>
            {
                if (Cache.HasValidCache(DataType))
                {
                    result = Cache.Get<T>(DataType);
                    Logger.LogInformation(Properties.Resources.FetchedFromCache, DataType.ToString());

                    return;
                }

                Result<T> fetchResult = await ExecuteFetch();

                if (fetchResult.IsSuccess)
                {
                    result = fetchResult.Value;
                    Cache.Store(DataType, result);
                }
                else
                {
                    result = Cache.Get<T>(DataType);
                }
            });
            await FetchingTask;

            return result;
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
        /// <returns>Fetched data as a JSON string.</returns>
        private async Task<Result<T>> ExecuteFetch()
        {
            Logger.LogInformation(Properties.Resources.StartFetching, DataType.ToString());

            Result<string> blobFetchResult = await BlobFetcher.Fetch(AzureBlobName);

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
