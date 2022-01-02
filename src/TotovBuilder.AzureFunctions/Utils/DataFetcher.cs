using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents a data fetcher.
    /// </summary>
    public class DataFetcher : IDataFetcher
    {
        /// <summary>
        /// Blob fetcher.
        /// </summary>
        private readonly IBlobDataFetcher BlobFetcher;

        /// <summary>
        /// Cache.
        /// </summary>
        private readonly ICache Cache;

        /// <summary>
        /// Configuration reader.
        /// </summary>
        private readonly IConfigurationReader ConfigurationReader;

        /// <summary>
        /// Name of the Azure Blob that stores item categories.
        /// </summary>
        private readonly string ItemCategoriesAzureBlobName;

        /// <summary>
        /// Name of the Azure Blob that stores items.
        /// </summary>
        private readonly string ItemsAzureBlobName;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<DataFetcher> Logger;

        /// <summary>
        /// Duration of the cache for market data in seconds.
        /// </summary>
        private readonly int MarketDataCacheDuration;

        /// <summary>
        /// Current market data fetching task.
        /// </summary>
        private Task MarketDataFetchingTask = Task.CompletedTask;

        /// <summary>
        /// Market data querier.
        /// </summary>
        private readonly IMarketDataFetcher MarketDataQuerier;

        /// <summary>
        /// Name of the Azure Blob that stores item presets.
        /// </summary>
        private readonly string PresetsAzureBlobName;

        /// <summary>
        /// Current static data fetching task.
        /// </summary>
        private Task StaticDataFetchingTask = Task.CompletedTask;

        /// <summary>
        /// Initializes an new instance of the <see cref="DataFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="querier">Querier.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="blobFetcher">Blob fetcher.</param>
        public DataFetcher(
            ILogger<DataFetcher> logger,
            IMarketDataFetcher querier,
            ICache cache,
            IConfigurationReader configurationReader,
            IBlobDataFetcher blobFetcher)
        {
            BlobFetcher = blobFetcher;
            Cache = cache;
            ConfigurationReader = configurationReader;
            Logger = logger;
            MarketDataQuerier = querier;

            ItemCategoriesAzureBlobName = ConfigurationReader.ReadString(Utils.ConfigurationReader.ItemCategoriesAzureBlobNameKey);
            ItemsAzureBlobName = ConfigurationReader.ReadString(Utils.ConfigurationReader.ItemsAzureBlobNameKey);
            MarketDataCacheDuration = ConfigurationReader.ReadInt(Utils.ConfigurationReader.MarketDataCacheDurationKey);
            PresetsAzureBlobName = ConfigurationReader.ReadString(Utils.ConfigurationReader.PresetsAzureBlobNameKey);
        }

        /// <inheritdoc/>
        public async Task<string> Fetch(DataType dataType)
        {
            if (dataType == DataType.MarketData)
            {
                if (!MarketDataFetchingTask.IsCompleted)
                {
                    Logger.LogInformation(string.Format(Properties.Resources.StartWaitingPreviousMarketDataFetching));

                    await MarketDataFetchingTask;

                    Logger.LogInformation(string.Format(Properties.Resources.EndWaitingPreviousMarketDataFetching));
                }
                else
                {
                    StartMarketDataFetching();
                    await FetchMarketData();
                    await EndMarketDataFetching();
                }
            }
            else
            {
                if (Cache.HasStaticDataCached)
                {
                    Logger.LogInformation(Properties.Resources.StaticDataFetchedFromCache);
                }
                else
                {
                    if (!StaticDataFetchingTask.IsCompleted)
                    {
                        Logger.LogInformation(string.Format(Properties.Resources.StartWaitingPreviousStaticDataFetching));

                        await StaticDataFetchingTask;

                        Logger.LogInformation(string.Format(Properties.Resources.EndWaitingPreviousStaticDataFetching));
                    }
                    else
                    {
                        Logger.LogInformation(string.Format(Properties.Resources.StartStaticDataFetching));

                        StartStaticDataFetching();                        
                        await Task.WhenAll(
                            FetchStaticData(DataType.ItemCategories),
                            FetchStaticData(DataType.Items),
                            FetchStaticData(DataType.Presets));
                        await EndStaticDataFetching();

                        Logger.LogInformation(string.Format(Properties.Resources.EndStaticDataFetching));
                    }
                }
            }

            string data = Cache.Get(dataType);

            return data;
        }

        /// <summary>
        /// Completes the market data fetching tasks.
        /// </summary>
        private async Task EndMarketDataFetching()
        {
            MarketDataFetchingTask.Start();
            await MarketDataFetchingTask;
        }

        /// <summary>
        /// Completes the static data fetching tasks.
        /// </summary>
        private async Task EndStaticDataFetching()
        {
            StaticDataFetchingTask!.Start();
            await StaticDataFetchingTask;
        }

        /// <summary>
        /// Fetches static data.
        /// </summary>
        /// <param name="dataType">Type of data to fetch.</param>
        private async Task FetchStaticData(DataType dataType)
        {
            string blobName = string.Empty;

            switch (dataType)
            {
                case DataType.ItemCategories:
                    blobName = ItemCategoriesAzureBlobName;
                    break;                
                case DataType.Items:
                    blobName = ItemsAzureBlobName;
                    break;               
                case DataType.Presets:
                    blobName = PresetsAzureBlobName;
                    break;
            }

            Result<string> blobFetchResult = await BlobFetcher.Fetch(blobName);

            if (blobFetchResult.IsSuccess)
            {
                Cache.Store(dataType, blobFetchResult.Value);
            }
            else
            {
                Cache.Remove(dataType);
            }

            return;
        }

        /// <summary>
        /// Fetches market data.
        /// </summary>
        /// <returns>Market data.</returns>
        private async Task FetchMarketData()
        {
            TimeSpan durationSinceLastFetch = DateTime.Now - Cache.LastMarketDataFetchDate;

            if (durationSinceLastFetch.TotalSeconds <= MarketDataCacheDuration)
            {          
                Logger.LogInformation(Properties.Resources.MarketDataFetchedFromCache);     
                
                return;
            }
            
            Logger.LogInformation(string.Format(Properties.Resources.StartMarketDataFetching));

            Result<string> marketDataResult = await MarketDataQuerier.Fetch();

            if (marketDataResult.IsSuccess)
            {
                Cache.Store(DataType.MarketData, marketDataResult.Value);
            }
            else
            {
                Cache.Remove(DataType.MarketData);
            }
            
            Logger.LogInformation(string.Format(Properties.Resources.EndMarketDataFetching));

            return;
        }

        /// <summary>
        /// Creates a market data fetching task.
        /// Any subsequent market data fetching task will wait for its completion before returning.
        /// </summary>
        private void StartMarketDataFetching()
        {
            MarketDataFetchingTask = new Task(() => { });
        }

        /// <summary>
        /// Creates a static data fetching task.
        /// Any subsequent static fetching task will wait for its completion before returning.
        /// </summary>
        private void StartStaticDataFetching()
        {
            StaticDataFetchingTask = new Task(() => { });
        }
    }
}
