using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a static data fetcher.
    /// </summary>
    public abstract class StaticDataFetcher<T> : IApiFetcher<T>
        where T: class
    {
        /// <summary>
        /// Configuration reader;
        /// </summary>
        protected readonly IConfigurationReader ConfigurationReader; 

        /// <summary>
        /// Type of data handled.
        /// </summary>
        protected abstract DataType DataType { get; }

        /// <summary>
        /// Name of the Azure Blob that stores the data.
        /// </summary>
        protected abstract string AzureBlobName { get; }

        /// <summary>
        /// Blob fetcher.
        /// </summary>
        private readonly IBlobFetcher BlobFetcher;

        /// <summary>
        /// Cache.
        /// </summary>
        private readonly ICache Cache;

        /// <summary>
        /// Fake task used to avoid launching multiple fetch operations at the same time.
        /// </summary>
        private Task FetchingTask = Task.CompletedTask;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDataFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobFetcher">Blob fetcher.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public StaticDataFetcher(ILogger logger, IBlobFetcher blobFetcher, IConfigurationReader configurationReader, ICache cache)
        {
            BlobFetcher = blobFetcher;
            Cache = cache;
            ConfigurationReader = configurationReader;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<T?> Fetch()
        {
            if (!FetchingTask.IsCompleted)
            {
                Logger.LogInformation(string.Format(Properties.Resources.StartWaitingForPreviousFetching, DataType.ToString()));

                await FetchingTask;

                Logger.LogInformation(string.Format(Properties.Resources.EndWaitingForPreviousFetching, DataType.ToString()));

                return Cache.Get<T>(DataType);
            }

            if (Cache.HasValidCache(DataType))
            {
                T? cachedValue = Cache.Get<T>(DataType);
                Logger.LogInformation(string.Format(Properties.Resources.FetchedFromCache, DataType.ToString()));

                return cachedValue;
            }
            
            T? result;
            FetchingTask = new Task(() => { });            
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
            
            FetchingTask.Start();
            await FetchingTask;

            return result;
        }

        /// <summary>
        /// Gets data from a the content of a fetch response.
        /// </summary>
        /// <param name="responseContent">Content of a fetch response.</param>
        /// <returns>Data.</returns>
        protected abstract Result<T> GetData(string responseContent);

        /// <summary>
        /// Executes the fetch operation.
        /// </summary>
        /// <returns>Fetched data as a JSON string.</returns>
        private async Task<Result<T>> ExecuteFetch()
        {
            Logger.LogInformation(string.Format(Properties.Resources.StartFetching, DataType.ToString()));      
            
            Result<string> blobFetchResult = await BlobFetcher.Fetch(AzureBlobName);

            if (!blobFetchResult.IsSuccess)
            {
                return blobFetchResult.ToResult<T>();
            }
            
            Result<T> result = GetData(blobFetchResult.Value);

            Logger.LogInformation(string.Format(Properties.Resources.EndFetching, DataType.ToString()));

            return result;
        }
    }
}
