using System.Text.Json;
using FluentResults;
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
        /// Tarkov values.
        /// </summary>
        private TarkovValues TarkovValues = new TarkovValues();

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
            Result<TarkovValues> tarkovValuesResult = await TarkovValuesFetcher.Fetch();

            if (tarkovValuesResult.IsFailed)
            {
                return tarkovValuesResult.ToResult();
            }

            TarkovValues = tarkovValuesResult.Value;

            List<Price> pricesAndBarters = [];
            Result<IEnumerable<Price>>? bartersResult = null;

            Task.WaitAll(
                Task.Run(() => pricesAndBarters.AddRange(GetPrices(responseContent))),
                BartersFetcher.Fetch().ContinueWith(r => bartersResult = r.Result));

            if (bartersResult!.IsSuccess)
            {
                pricesAndBarters.AddRange(bartersResult.Value);
            }

            return Result.Ok<IEnumerable<Price>>(pricesAndBarters);
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

                        Price price = new Price()
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
            Currency? mainCurrency = TarkovValues.Currencies.FirstOrDefault(c => c.MainCurrency);

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
