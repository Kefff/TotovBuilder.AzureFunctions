using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Utils;
using static TotovBuilder.AzureFunctions.Utils.JsonDeserializer;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a prices fetcher.
    /// </summary>
    public class PricesFetcher : ApiFetcher<IEnumerable<Price>>
    {
        /// <summary>
        ///  Game mode for which prices are fetched from the API.
        /// </summary>
        public GameMode GameMode { get; init; }

        /// <summary>
        ///  Language in which quests are fetched from the API.
        /// </summary>
        public string Language { get; init; }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Prices;
            }
        }

        /// <summary>
        /// API requester.
        /// </summary>
        private readonly IApiRequester ApiRequester;

        /// <summary>
        /// Barters fetcher.
        /// </summary>
        private readonly IBartersFetcher BartersFetcher;

        /// <summary>
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Quests fetcher.
        /// </summary>
        private readonly IQuestsFetcher QuestsFetcher;

        /// <summary>
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Traders fetcher.
        /// </summary>
        private readonly ITradersFetcher TradersFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="PricesFetcher"/> class.
        /// </summary>
        /// <param name="gameMode">Game mode for which prices are fetched.</param>
        /// <param name="language">Language in which quests are fetched.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="apiRequester">API requester.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="bartersFetcher">Barters fetcher.</param>
        /// <param name="questsFetcher">Quests fetcher.</param>
        /// <param name="tarkovValuesFetcher">Tarkov values fetcher.</param>
        /// <param name="tradersFetcher">Traders fetcher.</param>
        public PricesFetcher(
            GameMode gameMode,
            string language,
            ILogger<PricesFetcher> logger,
            IApiRequester apiRequester,
            IConfigurationWrapper configurationWrapper,
            IBartersFetcher bartersFetcher,
            IQuestsFetcher questsFetcher,
            ITarkovValuesFetcher tarkovValuesFetcher,
            ITradersFetcher tradersFetcher)
            : base(logger)
        {
            ApiRequester = apiRequester;
            BartersFetcher = bartersFetcher;
            ConfigurationWrapper = configurationWrapper;
            GameMode = gameMode;
            Language = language;
            QuestsFetcher = questsFetcher;
            TarkovValuesFetcher = tarkovValuesFetcher;
            TradersFetcher = tradersFetcher;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IEnumerable<Price>>> ExecuteFetch()
        {
            string itemsPath = BuildApiPath(ConfigurationWrapper.Values.ApiItemsPath, GameMode.ApiQueryValue);
            Task<Result<JsonElement>> itemsDataTask = ApiRequester.RequestData(itemsPath);
            Task<Result> questsTask = QuestsFetcher.Fetch();
            Task<Result> tarkovValuesTask = TarkovValuesFetcher.Fetch();
            Task<Result> tradersTask = TradersFetcher.Fetch();
            Task<Result> bartersTask = BartersFetcher.Fetch();

            await Task.WhenAll(itemsDataTask, questsTask, tarkovValuesTask, tradersTask, bartersTask);

            Result allTasksResult = Result.Merge(
                itemsDataTask.Result.ToResult(),
                questsTask.Result,
                tarkovValuesTask.Result,
                tradersTask.Result);

            if (allTasksResult.IsFailed)
            {
                return allTasksResult;
            }

            JsonElement itemsDataJson = itemsDataTask.Result.Value;

            Result<JsonElement> itemsResult = ItemsData.GetItems(itemsDataJson, itemsPath, Logger);

            if (itemsResult.IsFailed)
            {
                return itemsResult.ToResult();
            }

            JsonElement itemsJson = itemsResult.Value;

            List<Price> pricesAndBarters = [.. GetPrices(itemsDataJson, itemsJson)];

            if (bartersTask.Result.IsSuccess)
            {
                TranformBartersWithCurrency(BartersFetcher.FetchedData!, pricesAndBarters);
                pricesAndBarters.AddRange(BartersFetcher.FetchedData!);
            }

            return Result.Ok<IEnumerable<Price>>(pricesAndBarters);
        }

        /// <summary>
        /// Gets the flea market price of an item.
        /// </summary>
        /// <param name="itemJson">Json element representing the item.</param>
        /// <param name="itemId">ID of the item.</param>
        /// <param name="fleaMarketName">Name of the flea market merchant.</param>
        /// <param name="mainCurrency">Main currency.</param>
        /// <returns>Flea market price, or <c>null</c> when the item cannot be bought on the flea market.</returns>
        private static Price? GetFleaMarketPrice(JsonElement itemJson, string itemId, string fleaMarketName, Currency mainCurrency)
        {
            if (TryDeserializeArray(itemJson, "types", out JsonElement.ArrayEnumerator typesJson)
                && typesJson.Any(t => t.ValueKind == JsonValueKind.String && t.GetString() == "noFlea"))
            {
                return null;
            }

            if (!TryDeserializeDouble(itemJson, "avg24hPrice", out double averagePrice)
                || averagePrice <= 0)
            {
                return null;
            }

            Price price = new()
            {
                CurrencyName = mainCurrency.Name,
                ItemId = itemId,
                Merchant = fleaMarketName,
                Value = averagePrice,
                ValueInMainCurrency = averagePrice
            };

            if (TryDeserializeDouble(itemJson, "minLevelForFlea", out double minPlayerLevel)
                && minPlayerLevel > 0)
            {
                price.MerchantLevel = (int)minPlayerLevel;
            }

            return price;
        }

        /// <summary>
        /// Gets prices.
        /// </summary>
        /// <param name="itemsDataJson">Json element representing the items data.</param>
        /// <param name="itemsJson">Json element representing the items.</param>
        /// <returns>Prices.</returns>
        private IEnumerable<Price> GetPrices(JsonElement itemsDataJson, JsonElement itemsJson)
        {
            List<Price> prices = [];
            Currency? mainCurrency = TarkovValuesFetcher.FetchedData!.Currencies.FirstOrDefault(c => c.MainCurrency);
            string? fleaMarketName = null;

            if (TryDeserializeObject(itemsDataJson, "fleaMarket", out JsonElement fleaMarketJson)
                && TryDeserializeBoolean(fleaMarketJson, "enabled", out bool isFleaMarketEnabled)
                && isFleaMarketEnabled
                && TryDeserializeString(fleaMarketJson, "normalizedName", out string normalizedName))
            {
                fleaMarketName = normalizedName;
            }

            foreach (JsonProperty itemJson in itemsJson.EnumerateObject())
            {
                try
                {
                    string itemId = itemJson.Value.GetProperty("id").GetString()!;

                    foreach (JsonElement offerJson in itemJson.Value.GetProperty("buyFromTrader").EnumerateArray())
                    {
                        int value = offerJson.GetProperty("price").GetInt32();

                        if (value == 0)
                        {
                            continue;
                        }

                        Price price = new()
                        {
                            CurrencyName = offerJson.GetProperty("currency").GetString()!,
                            ItemId = itemId,
                            Merchant = TradersFetcher.GetMerchantName(offerJson.GetProperty("trader").GetString()!),
                            Value = value,
                            ValueInMainCurrency = offerJson.GetProperty("priceRUB").GetInt32()
                        };

                        if (TryDeserializeDouble(offerJson, "minTraderLevel", out double minTraderLevel)
                            && minTraderLevel > 0)
                        {
                            price.MerchantLevel = (int)minTraderLevel;
                        }

                        if (TryDeserializeString(offerJson, "taskUnlock", out string taskId))
                        {
                            price.Quest = QuestsFetcher.GetQuest(taskId);
                        }

                        prices.Add(price);
                    }

                    if (fleaMarketName != null && mainCurrency != null)
                    {
                        Price? fleaMarketPrice = GetFleaMarketPrice(itemJson.Value, itemId, fleaMarketName, mainCurrency);

                        if (fleaMarketPrice != null)
                        {
                            prices.Add(fleaMarketPrice);
                        }
                    }
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.PriceDeserializationError, e);
                    Logger.LogError(error);
                }
            }

            // Adding a price to the main currency item
            if (mainCurrency != null)
            {
                prices.Add(new Price()
                {
                    CurrencyName = mainCurrency.Name,
                    ItemId = mainCurrency.ItemId!,
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Value = 1,
                    ValueInMainCurrency = 1
                });
            }

            return prices;
        }

        /// <summary>
        /// Transforms barters into prices when the barter is only constituted of one currency.
        /// </summary>
        /// <remarks>This can be the case for items sold with GP coins.</remarks>
        /// <param name="barters">List of barters.</param>
        /// <param name="prices">List of prices.</param>
        private void TranformBartersWithCurrency(IEnumerable<Price> barters, IEnumerable<Price> prices)
        {
            foreach (Price barter in barters.Where(b => b.BarterItems.Length == 1))
            {
                // When the price is considered a barter but it uses a currency (for example GP coins)
                // we do not consider it to be a barter
                BarterItem barterItem = barter.BarterItems[0];
                Currency? currency = TarkovValuesFetcher.FetchedData!.Currencies.FirstOrDefault(c => c.ItemId == barterItem.ItemId);

                if (currency == null)
                {
                    continue;
                }

                Price? currencyPrice = prices.FirstOrDefault(p => p.ItemId == currency.ItemId);
                barter.BarterItems = [];
                barter.CurrencyName = currency.Name;
                barter.Value = barterItem.Quantity;
                barter.ValueInMainCurrency = barterItem.Quantity * (currencyPrice?.ValueInMainCurrency ?? 0);
            }
        }
    }
}
