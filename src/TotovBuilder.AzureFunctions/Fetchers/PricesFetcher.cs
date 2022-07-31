using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;
using TotovBuilder.AzureFunctions.Models.Items;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a prices fetcher.
    /// </summary>
    public class PricesFetcher : ApiFetcher<IEnumerable<Item>>, IPricesFetcher
    {
        /// <inheritdoc/>
        protected override string ApiQuery => AzureFunctionsConfigurationReader.Values.ApiPricesQuery;
        
        /// <inheritdoc/>
        protected override DataType DataType => DataType.Prices;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public PricesFetcher(ILogger logger, IHttpClientWrapperFactory httpClientWrapperFactory, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, ICache cache)
            : base(logger, httpClientWrapperFactory, azureFunctionsConfigurationReader, cache)
        {
        }
        
        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<Item>>> DeserializeData(string responseContent)
        {
            List<Item> items = new List<Item>();

            JsonElement pricesJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement itemJson in pricesJson.EnumerateArray())
            {
                List<Price> prices = new List<Price>();

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
                            Value = value,
                            ValueInMainCurrency = priceJson.GetProperty("priceRUB").GetInt32()
                        };

                        if (priceJson.GetProperty("vendor").TryGetProperty("trader", out JsonElement traderJson))
                        {
                            price.Merchant = traderJson.GetProperty("normalizedName").GetString();
                        }
                        else
                        {
                            price.Merchant = priceJson.GetProperty("vendor").GetProperty("normalizedName").GetString();
                        }

                        if (priceJson.GetProperty("vendor").TryGetProperty("minTraderLevel", out JsonElement minTraderLevel))
                        {
                            price.MerchantLevel = minTraderLevel.GetInt32();
                        }

                        if (priceJson.GetProperty("vendor").TryGetProperty("taskUnlock", out JsonElement taskUnlockJson) && taskUnlockJson.ValueKind != JsonValueKind.Null)
                        {
                            price.QuestId = taskUnlockJson.GetProperty("id").GetString();
                        }

                        prices.Add(price);
                    }
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.PriceDeserializationError, e);
                    Logger.LogError(error);
                }

                if (prices.Count == 0)
                {
                    continue;
                }
                    
                Item item = new Item()
                {
                    Id = itemJson.GetProperty("id").GetString(),
                    Prices = prices.ToArray()
                };

                items.Add(item);
            }

            return Task.FromResult(Result.Ok(items.AsEnumerable()));
        }
    }
}
