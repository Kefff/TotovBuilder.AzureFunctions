﻿using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
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
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="tarkovValuesFetcher">Tarkov values fetcher.</param>
        public PricesFetcher(
            ILogger<PricesFetcher> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationWrapper configurationWrapper,
            ITarkovValuesFetcher tarkovValuesFetcher)
            : base(logger, httpClientWrapperFactory, configurationWrapper)
        {
            TarkovValuesFetcher = tarkovValuesFetcher;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IEnumerable<Price>>> DeserializeData(string responseContent)
        {
            List<Price> prices = new List<Price>();
            Result<TarkovValues> tarkovValuesResult = await TarkovValuesFetcher.Fetch();

            if (tarkovValuesResult.IsFailed)
            {
                return tarkovValuesResult.ToResult();
            }

            TarkovValues tarkovValues = tarkovValuesResult.Value;
            JsonElement pricesJson = JsonDocument.Parse(responseContent).RootElement;

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
            Currency? mainCurrency = tarkovValues.Currencies.FirstOrDefault(c => c.MainCurrency);

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

            return Result.Ok(prices.AsEnumerable());
        }
    }
}
