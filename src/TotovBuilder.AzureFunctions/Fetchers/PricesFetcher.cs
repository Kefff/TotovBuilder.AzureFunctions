using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
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
        protected override string ApiQuery => AzureFunctionsConfigurationWrapper.Values.ApiPricesQuery;
        
        /// <inheritdoc/>
        protected override DataType DataType => DataType.Prices;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="azureFunctionsConfigurationWrapper">Azure Functions configuration wrapper.</param>
        /// <param name="cache">Cache.</param>
        public PricesFetcher(ILogger<PricesFetcher> logger, IHttpClientWrapperFactory httpClientWrapperFactory, IAzureFunctionsConfigurationWrapper azureFunctionsConfigurationWrapper, ICache cache)
            : base(logger, httpClientWrapperFactory, azureFunctionsConfigurationWrapper, cache)
        {
        }
        
        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<Price>>> DeserializeData(string responseContent)
        {
            List<Price> prices = new List<Price>();

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
                            CurrencyName = priceJson.GetProperty("currency").GetString(),
                            ItemId = itemJson.GetProperty("id").GetString(),
                            Value = value,
                            ValueInMainCurrency = priceJson.GetProperty("priceRUB").GetInt32()
                        };

                        JsonElement vendorJson = priceJson.GetProperty("vendor");

                        if (vendorJson.TryGetProperty("trader", out JsonElement traderJson))
                        {
                            price.Merchant = traderJson.GetProperty("normalizedName").GetString();
                        }
                        else
                        {
                            price.Merchant = priceJson.GetProperty("vendor").GetProperty("normalizedName").GetString();
                        }

                        if (vendorJson.TryGetProperty("minTraderLevel", out JsonElement minTraderLevelJson))
                        {
                            int minTraderLevel = minTraderLevelJson.GetInt32();

                            if (minTraderLevel > 0)
                            {
                                price.MerchantLevel = minTraderLevel;
                            }
                        }

                        if (vendorJson.TryGetProperty("taskUnlock", out JsonElement taskUnlockJson) && taskUnlockJson.ValueKind != JsonValueKind.Null)
                        {
                            price.Quest = new Quest()
                            {
                                Id = taskUnlockJson.GetProperty("id").GetString(),
                                Name = taskUnlockJson.GetProperty("name").GetString(),
                                WikiLink = taskUnlockJson.GetProperty("wikiLink").GetString()
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

            return Task.FromResult(Result.Ok(prices.AsEnumerable()));
        }
    }
}
