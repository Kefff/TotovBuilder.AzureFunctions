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
    public class LocalizedItemsFetcher : ApiFetcher<IEnumerable<LocalizedItems>>, ILocalizedItemsFetcher
    {
        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Items;
            }
        }

        private readonly IApiRequester ApiRequester;
        private readonly IConfigurationWrapper ConfigurationWrapper;
        private readonly IItemCategoriesFetcher ItemCategoriesFetcher;
        private readonly ILogger<ItemsFetcher> ItemsFetcherLogger;
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        public LocalizedItemsFetcher(
            ILogger<ItemsFetcher> logger,
            IApiRequester apiRequester,
            IConfigurationWrapper configurationWrapper,
            IItemCategoriesFetcher itemCategoriesFetcher,
            ITarkovValuesFetcher tarkovValuesFetcher)
            : base(logger)
        {
            ApiRequester = apiRequester;
            ConfigurationWrapper = configurationWrapper;
            ItemCategoriesFetcher = itemCategoriesFetcher;
            ItemsFetcherLogger = logger;
            TarkovValuesFetcher = tarkovValuesFetcher;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IEnumerable<LocalizedItems>>> ExecuteFetch()
        {
            List<ItemsFetcher> itemFetchers = [];
            List<Task<Result>> fetchTasks = [];

            foreach (string language in ConfigurationWrapper.Values.ItemsLanguages)
            {
                ItemsFetcher itemsFetcher = new(
                    language,
                    ItemsFetcherLogger,
                    ApiRequester,
                    ConfigurationWrapper,
                    ItemCategoriesFetcher,
                    TarkovValuesFetcher);
                itemFetchers.Add(itemsFetcher);
                fetchTasks.Add(itemsFetcher.Fetch());
            }

            await Task.WhenAll(fetchTasks);
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
                return Result.Fail(string.Format(Properties.Resources.NoDataFetched, DataType.ToString()));
            }

            return Result.Ok(fetchedData.AsEnumerable());
        }
    }
}
