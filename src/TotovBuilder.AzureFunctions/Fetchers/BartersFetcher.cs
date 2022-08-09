using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a barters fetcher.
    /// </summary>
    public class BartersFetcher : ApiFetcher<IEnumerable<Item>>, IBartersFetcher
    {
        /// <inheritdoc/>
        protected override string ApiQuery => AzureFunctionsConfigurationWrapper.Values.ApiBartersQuery;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.Barters;

        /// <summary>
        /// Initializes a new instance of the <see cref="BartersFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="azureFunctionsConfigurationWrapper">Azure Functions configuration wrapper.</param>
        /// <param name="cache">Cache.</param>
        public BartersFetcher(ILogger<BartersFetcher> logger, IHttpClientWrapperFactory httpClientWrapperFactory, IAzureFunctionsConfigurationWrapper azureFunctionsConfigurationWrapper, ICache cache)
            : base(logger, httpClientWrapperFactory, azureFunctionsConfigurationWrapper, cache)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<Item>>> DeserializeData(string responseContent)
        {
            List<Item> items = new List<Item>();

            JsonElement bartersJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement barterJson in bartersJson.EnumerateArray())
            {
                try
                {
                    List<BarterItem> barterItems = new List<BarterItem>();
                    string merchant = barterJson.GetProperty("trader").GetProperty("normalizedName").GetString();
                    int merchantLevel = barterJson.GetProperty("level").GetInt32();

                    JsonElement questJson = barterJson.GetProperty("taskUnlock");

                    if (!TryDeserializeString(questJson, "id", out string? questId))
                    {
                        questId = null;
                    }

                    foreach (JsonElement baterItemJson in barterJson.GetProperty("requiredItems").EnumerateArray())
                    {
                        barterItems.Add(new BarterItem()
                        {
                            ItemId = baterItemJson.GetProperty("item").GetProperty("id").GetString(),
                            Quantity = baterItemJson.GetProperty("quantity").GetInt32()
                        });
                    }

                    foreach (JsonElement itemJson in barterJson.GetProperty("rewardItems").EnumerateArray())
                    {
                        Item item;
                        int quantity = itemJson.GetProperty("quantity").GetInt32();
                        string id = itemJson.GetProperty("item").GetProperty("id").GetString();
                        Price barter = new Price()
                        {
                            BarterItems = barterItems.ToArray(),
                            CurrencyName = "barter",
                            Merchant = merchant,
                            MerchantLevel = merchantLevel,
                            QuestId = questId
                        };

                        if (items.Any(i => i.Id == id))
                        {
                            item = items.Single(i => i.Id == id);

                            List<Price> prices = new List<Price>(item.Prices)
                            {
                                barter
                            };
                            item.Prices = prices.ToArray();
                        }
                        else
                        {
                            item = new Item()
                            {
                                Id = id,
                                Prices = new Price[]
                                {
                                    barter
                                }
                            };

                            items.Add(item);
                        }

                        if (quantity > 1)
                        {
                            foreach (Price price in item.Prices)
                            {
                                foreach (BarterItem barterItem in price.BarterItems)
                                {
                                    // Dividing the required number of items by the amount of received items
                                    barterItem.Quantity /= quantity;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.BarterDeserializationError, e);
                    Logger.LogError(error);
                }
            }

            return Task.FromResult(Result.Ok(items.AsEnumerable()));
        }
    }
}
