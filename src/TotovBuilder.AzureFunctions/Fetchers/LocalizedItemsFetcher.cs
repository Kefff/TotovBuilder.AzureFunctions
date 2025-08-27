using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Utils;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a fetcher that fetches localized items.
    /// </summary>
    public class LocalizedItemsFetcher : ILocalizedItemsFetcher
    {
        private readonly ILogger<ItemsFetcher> Logger;
        private readonly IHttpClientWrapperFactory HttpClientWrapperFactory;
        private readonly IConfigurationWrapper ConfigurationWrapper;
        private readonly IItemCategoriesFetcher ItemCategoriesFetcher;
        private readonly IItemMissingPropertiesFetcher ItemMissingPropertiesFetcher;
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <inheritdoc/>
        public IEnumerable<LocalizedItems>? FetchedData { get; private set; } = null;

        public LocalizedItemsFetcher(
            ILogger<ItemsFetcher> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationWrapper configurationWrapper,
            IItemCategoriesFetcher itemCategoriesFetcher,
            IItemMissingPropertiesFetcher itemMissingPropertiesFetcher,
            ITarkovValuesFetcher tarkovValuesFetcher)
        {
            Logger = logger;
            HttpClientWrapperFactory = httpClientWrapperFactory;
            ConfigurationWrapper = configurationWrapper;
            ItemCategoriesFetcher = itemCategoriesFetcher;
            ItemMissingPropertiesFetcher = itemMissingPropertiesFetcher;
            TarkovValuesFetcher = tarkovValuesFetcher;
        }

        /// <inheritdoc/>
        public Task<Result> Fetch()
        {
            if (FetchedData != null)
            {
                return Task.FromResult(Result.Ok());
            }

            List<ItemsFetcher> itemFetchers = [];
            List<Task<Result>> fetchTasks = [];

            foreach (string language in ConfigurationWrapper.Values.Languages)
            {
                ItemsFetcher itemsFetcher = new(
                    language,
                    Logger,
                    HttpClientWrapperFactory,
                    ConfigurationWrapper,
                    ItemCategoriesFetcher,
                    ItemMissingPropertiesFetcher,
                    TarkovValuesFetcher);
                itemFetchers.Add(itemsFetcher);
                fetchTasks.Add(itemsFetcher.Fetch());
            }

            Task.WaitAll([.. fetchTasks]);
            List<LocalizedItems> fetchedData = [];

            foreach (ItemsFetcher itemFetcher in itemFetchers.Where(ife => ife.FetchedData != null))
            {
                fetchedData.Add(new LocalizedItems()
                {
                    Items = [.. itemFetcher.FetchedData!],
                    Language = itemFetcher.Language
                });
            }

            if (fetchedData.Count == 0)
            {
                return Task.FromResult(Result.Fail(string.Format(Properties.Resources.NoDataFetched, DataType.Items.ToString())));
            }

            FetchedData = fetchedData;

            return Task.FromResult(Result.Ok());
        }
    }
}
