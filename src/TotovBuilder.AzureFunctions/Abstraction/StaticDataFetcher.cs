using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace TotovBuilder.AzureFunctions.Abstraction
{
    /// <summary>
    /// Represents a static data fetcher.
    /// </summary>
    public abstract class StaticDataFetcher : IApiFetcher
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
        private readonly IBlobDataFetcher BlobFetcher;

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
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public StaticDataFetcher(ILogger logger, IBlobDataFetcher blobDataFetcher, IConfigurationReader configurationReader, ICache cache)
        {
            BlobFetcher = blobDataFetcher;
            Cache = cache;
            ConfigurationReader = configurationReader;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> Fetch()
        {
            if (Cache.HasValidCache(DataType))
            {
                Logger.LogInformation(string.Format(Properties.Resources.FetchedFromCache, DataType.ToString()));

                return Cache.Get(DataType);
            }

            if (!FetchingTask.IsCompleted)
            {
                Logger.LogInformation(string.Format(Properties.Resources.StartWaitingForPreviousFetching, DataType.ToString()));

                await FetchingTask;

                Logger.LogInformation(string.Format(Properties.Resources.EndWaitingForPreviousFetching, DataType.ToString()));

                return Cache.Get(DataType);
            }

            Logger.LogInformation(string.Format(Properties.Resources.StartFetching, DataType.ToString()));
            
            FetchingTask = new Task(() => { });
            Result<string> blobFetchResult = await BlobFetcher.Fetch(AzureBlobName);

            if (blobFetchResult.IsSuccess)
            {
                Cache.Store(DataType, blobFetchResult.Value);
            }
            
            FetchingTask.Start();
            await FetchingTask;

            Logger.LogInformation(string.Format(Properties.Resources.EndFetching, DataType.ToString()));

            return blobFetchResult.Value;
        }
    }
}
