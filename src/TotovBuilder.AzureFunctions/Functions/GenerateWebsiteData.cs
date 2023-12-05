using FluentResults;
using Microsoft.Azure.Functions.Worker;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Utils;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that generates data for the website and uploads the generated files to the website Azure blob storage.
    /// </summary>
    public class GenerateWebsiteData
    {
        /// <summary>
        /// Azure blob manager.
        /// </summary>
        private readonly IAzureBlobManager AzureBlobManager;

        /// <summary>
        /// Configuration loader.
        /// </summary>
        private readonly IConfigurationLoader ConfigurationLoader;

        /// <summary>
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Changelog fetcher.
        /// </summary>
        private readonly IChangelogFetcher ChangelogFetcher;

        /// <summary>
        /// Item categories fetcher.
        /// </summary>
        private readonly IItemCategoriesFetcher ItemCategoriesFetcher;

        /// <summary>
        /// Items fetcher.
        /// </summary>
        private readonly IItemsFetcher ItemsFetcher;

        /// <summary>
        /// Presets fetcher.
        /// </summary>
        private readonly IPresetsFetcher PresetsFetcher;

        /// <summary>
        /// Prices fetcher.
        /// </summary>
        private readonly IPricesFetcher PricesFetcher;

        /// <summary>
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Website configuration fetcher.
        /// </summary>
        private readonly IWebsiteConfigurationFetcher WebsiteConfigurationFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateWebsiteData"/> class.
        /// </summary>
        /// <param name="configurationLoader">Configuration loader.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="azureBlobManager">Azure blob manager.</param>
        /// <param name="changelogFetcher">Changelog fetcher.</param>.
        /// <param name="itemCategoriesFetcher">Item categories fetcher.</param>
        /// <param name="itemsFetcher">Item fetcher.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="presetsFetcher">Presets fetcher.</param>
        /// <param name="pricesFetcher">Prices fetcher.</param>
        /// <param name="tarkovValuesFetcher">Tarkov values fetcher.</param>
        /// <param name="websiteConfigurationFetcher">Website configuration fetcher.</param>
        public GenerateWebsiteData(
            IConfigurationLoader configurationLoader,
            IConfigurationWrapper configurationWrapper,
            IAzureBlobManager azureBlobManager,
            IChangelogFetcher changelogFetcher,
            IItemCategoriesFetcher itemCategoriesFetcher,
            IItemsFetcher itemsFetcher,
            IPresetsFetcher presetsFetcher,
            IPricesFetcher pricesFetcher,
            ITarkovValuesFetcher tarkovValuesFetcher,
            IWebsiteConfigurationFetcher websiteConfigurationFetcher)
        {
            ConfigurationLoader = configurationLoader;
            ConfigurationWrapper = configurationWrapper;
            AzureBlobManager = azureBlobManager;
            ChangelogFetcher = changelogFetcher;
            ItemCategoriesFetcher = itemCategoriesFetcher;
            ItemsFetcher = itemsFetcher;
            PresetsFetcher = presetsFetcher;
            PricesFetcher = pricesFetcher;
            TarkovValuesFetcher = tarkovValuesFetcher;
            WebsiteConfigurationFetcher = websiteConfigurationFetcher;
        }

        [Function("GenerateWebsiteData")]
        public async Task Run([TimerTrigger("%TOTOVBUILDER_GenerateWebsiteDataSchedule%")] ScheduleTrigger _)
        {
            await ConfigurationLoader.Load();
            Task.WaitAll(
                FetchAndUpload(ChangelogFetcher, ConfigurationWrapper.Values.WebsiteChangelogBlobName),
                FetchAndUpload(ItemCategoriesFetcher, ConfigurationWrapper.Values.WebsiteItemCategoriesBlobName),
                FetchAndUpload(ItemsFetcher, ConfigurationWrapper.Values.WebsiteItemsBlobName),
                FetchAndUpload(PresetsFetcher, ConfigurationWrapper.Values.WebsitePresetsBlobName),
                FetchAndUpload(PricesFetcher, ConfigurationWrapper.Values.WebsitePricesBlobName),
                FetchAndUpload(TarkovValuesFetcher, ConfigurationWrapper.Values.WebsiteTarkovValuesBlobName),
                FetchAndUpload(WebsiteConfigurationFetcher, ConfigurationWrapper.Values.WebsiteWebsiteConfigurationBlobName));
        }

        /// <summary>
        /// Fetches data generated for the website and uploads it to a blob storage.
        /// </summary>
        /// <typeparam name="TData">Type of data.</typeparam>
        /// <param name="fetcher">Fetcher.</param>
        /// <param name="azureBlobName">Blob name.</param>
        /// <returns></returns>
        private async Task FetchAndUpload<TData>(IApiFetcher<TData> fetcher, string azureBlobName)
            where TData : class
        {
            Result<TData> result = await fetcher.Fetch();

            if (result.IsFailed)
            {
                return;
            }

            await AzureBlobManager.Update(ConfigurationWrapper.Values.AzureBlobStorageWebsiteDataContainerName, azureBlobName, result.Value);
        }
    }
}
