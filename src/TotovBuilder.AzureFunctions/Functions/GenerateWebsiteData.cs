using System.Collections;
using System.Text.Json;
using FluentResults;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Utils;
using TotovBuilder.Model.Items;

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
        /// Logger.
        /// </summary>
        private readonly ILogger<GenerateWebsiteData> Logger;

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
            ILogger<GenerateWebsiteData> logger,
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
            Logger = logger;
            PresetsFetcher = presetsFetcher;
            PricesFetcher = pricesFetcher;
            TarkovValuesFetcher = tarkovValuesFetcher;
            WebsiteConfigurationFetcher = websiteConfigurationFetcher;
        }

        [Function("GenerateWebsiteData")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Azure Functions do not support discard (\"_\") parameters when the parameter is not used.")]
#if DEBUG
        public async Task Run([TimerTrigger("%TOTOVBUILDER_GenerateWebsiteDataSchedule%", RunOnStartup = true)] ScheduleTrigger scheduleTrigger)
#else
        public async Task Run([TimerTrigger("%TOTOVBUILDER_GenerateWebsiteDataSchedule%")] ScheduleTrigger scheduleTrigger)
#endif
        {
            await ConfigurationLoader.Load();
            Task.WaitAll(
                FetchAndUpload(ChangelogFetcher, ConfigurationWrapper.Values.WebsiteChangelogBlobName),
                FetchAndUpload(
                    ItemCategoriesFetcher,
                    ConfigurationWrapper.Values.WebsiteItemCategoriesBlobName,
                    TransformItemCategories),
                FetchAndUpload(ItemsFetcher, ConfigurationWrapper.Values.WebsiteItemsBlobName),
                FetchAndUpload(PresetsFetcher, ConfigurationWrapper.Values.WebsitePresetsBlobName),
                FetchAndUpload(PricesFetcher, ConfigurationWrapper.Values.WebsitePricesBlobName),
                FetchAndUpload(TarkovValuesFetcher, ConfigurationWrapper.Values.WebsiteTarkovValuesBlobName),
                FetchAndUpload(WebsiteConfigurationFetcher, ConfigurationWrapper.Values.WebsiteWebsiteConfigurationBlobName));
        }

        /// <summary>
        /// Fetches data for the website and uploads it to a blob storage.
        /// </summary>
        /// <typeparam name="TData">Type of data.</typeparam>
        /// <param name="fetcher">Fetcher.</param>
        /// <param name="azureBlobName">Blob name.</param>
        private async Task FetchAndUpload<TData>(IApiFetcher<TData> fetcher, string azureBlobName)
            where TData : class
        {
            await FetchAndUpload(fetcher, azureBlobName, (TData input) => input);
        }

        /// <summary>
        /// Fetches data for the website, transforms and uploads it to a blob storage.
        /// </summary>
        /// <typeparam name="TData">Type of data.</typeparam>
        /// <param name="fetcher">Fetcher.</param>
        /// <param name="azureBlobName">Blob name.</param>
        /// <param name="transformationFunction">Function for transforming data before uploading.</param>
        private async Task FetchAndUpload<TIn, TOut>(IApiFetcher<TIn> fetcher, string azureBlobName, Func<TIn, TOut> transformationFunction)
            where TIn : class
            where TOut : class
        {
            Result<TIn> fetchResult = await fetcher.Fetch();

            if (fetchResult.IsFailed)
            {
                return;
            }

            TOut transformedData = transformationFunction(fetchResult.Value);

            JsonSerializerOptions serializationOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string serializedData;

            if (typeof(IEnumerable).IsAssignableFrom(typeof(TOut)))
            {
                serializedData = JsonSerializer.Serialize(
                    transformedData as IEnumerable<object>, // Cast required otherwise properties of classes inheriting from Item are not serialized. See https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0
                    serializationOptions);
            }
            else
            {
                serializedData = JsonSerializer.Serialize(
                    transformedData as object, // Cast required otherwise properties of classes inheriting from Item are not serialized. See https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0
                    serializationOptions);
            }

            Logger.LogInformation(Properties.Resources.StartUpdating, azureBlobName);

            Result updateResult = await AzureBlobManager.Update(azureBlobName, serializedData);

            if (updateResult.IsFailed)
            {
                return;
            }

            Logger.LogInformation(Properties.Resources.EndUpdating, azureBlobName);
        }

        /// <summary>
        /// Transform item categories into a list of item category IDs.
        /// </summary>
        /// <param name="itemCategories">Item categories.</param>
        /// <returns>Item category IDs.</returns>
        private IEnumerable<string> TransformItemCategories(IEnumerable<ItemCategory> itemCategories)
        {
            return itemCategories.Select(ic => ic.Id);
        }
    }
}
