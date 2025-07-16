using System.Text.Json;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a prices fetcher.
    /// </summary>
    public class PricesFetcher : ApiFetcher<IEnumerable<Price>>, IPricesFetcher
    {
        /// <inheritdoc/>
        protected override string ApiQuery
        {
            get
            {
                return ConfigurationWrapper.Values.ApiPricesQuery;
            }
        }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Prices;
            }
        }

        /// <summary>
        /// Barters fetcher.
        /// </summary>
        private readonly IBartersFetcher BartersFetcher;

        /// <summary>
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="bartersFetcher">Barters fetcher.</param>
        /// <param name="tarkovValuesFetcher">Tarkov values fetcher.</param>
        public PricesFetcher(
            ILogger<PricesFetcher> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationWrapper configurationWrapper,
            IBartersFetcher bartersFetcher,
            ITarkovValuesFetcher tarkovValuesFetcher)
            : base(logger, httpClientWrapperFactory, configurationWrapper)
        {
            BartersFetcher = bartersFetcher;
            TarkovValuesFetcher = tarkovValuesFetcher;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IEnumerable<Price>>> DeserializeData(string responseContent)
        {
            Result tarkovValuesResult = await TarkovValuesFetcher.Fetch();

            if (tarkovValuesResult.IsFailed)
            {
                return tarkovValuesResult;
            }

            List<Price> pricesAndBarters = [];
            Result<IEnumerable<Price>>? bartersResult = null;

            Task.WaitAll(
                Task.Run(() => pricesAndBarters.AddRange(GetPrices(responseContent))),
                BartersFetcher.Fetch().ContinueWith(r => bartersResult = r.Result));

            if (bartersResult!.IsSuccess)
            {
                TranformBartersWithCurrency(BartersFetcher.FetchedData!, pricesAndBarters);
                pricesAndBarters.AddRange(BartersFetcher.FetchedData!);
            }

            return Result.Ok<IEnumerable<Price>>(pricesAndBarters);
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

        /// <summary>
        /// Gets prices.
        /// </summary>
        /// <param name="pricesResponseContent">Content of the price fetch response.</param>
        /// <returns>Prices.</returns>
        private IEnumerable<Price> GetPrices(string pricesResponseContent)
        {
            List<Price> prices = [];
            JsonElement pricesJson = JsonDocument.Parse(pricesResponseContent).RootElement;

            foreach (JsonElement itemJson in pricesJson.EnumerateArray())
            {
                try
                {
                    foreach (JsonElement priceJson in itemJson.GetProperty("buyFor").EnumerateArray())
                    {
                        int value = priceJson.GetProperty("price").GetInt32();

                        if (value == 0)
                        {
                            continue;
                        }

                        Price price = new()
                        {
                            CurrencyName = priceJson.GetProperty("currency").GetString()!,
                            ItemId = itemJson.GetProperty("id").GetString()!,
                            Value = value,
                            ValueInMainCurrency = priceJson.GetProperty("priceRUB").GetInt32()
                        };

                        JsonElement vendorJson = priceJson.GetProperty("vendor");

                        if (vendorJson.TryGetProperty("trader", out JsonElement traderJson))
                        {
                            price.Merchant = traderJson.GetProperty("normalizedName").GetString()!;
                        }
                        else
                        {
                            price.Merchant = priceJson.GetProperty("vendor").GetProperty("normalizedName").GetString()!;
                        }

                        if (vendorJson.TryGetProperty("minTraderLevel", out JsonElement minTraderLevelJson))
                        {
                            int minTraderLevel = minTraderLevelJson.GetInt32();

                            if (minTraderLevel > 0)
                            {
                                price.MerchantLevel = minTraderLevel;
                            }
                        }

                        if (TryDeserializeObject(vendorJson, "taskUnlock", out JsonElement taskUnlockJson))
                        {
                            price.Quest = new Quest()
                            {
                                Id = taskUnlockJson.GetProperty("id").GetString()!,
                                Name = taskUnlockJson.GetProperty("name").GetString()!,
                                WikiLink = taskUnlockJson.GetProperty("wikiLink").GetString()!
                            };
                        }

                        prices.Add(price);
                    }
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.PriceDeserializationError, e);
                    Logger.LogError(error);
                }
            }

            // Adding a price to the main currency item
            Currency? mainCurrency = TarkovValuesFetcher.FetchedData!.Currencies.FirstOrDefault(c => c.MainCurrency);

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

            return prices.AsEnumerable();
        }
    }
}
