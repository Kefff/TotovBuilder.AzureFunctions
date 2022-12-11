using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a barters fetcher.
    /// </summary>
    public class BartersFetcher : ApiFetcher<IEnumerable<Price>>, IBartersFetcher
    {
        /// <inheritdoc/>
        protected override string ApiQuery => AzureFunctionsConfigurationCache.Values.ApiBartersQuery;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.Barters;

        /// <summary>
        /// Initializes a new instance of the <see cref="BartersFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="azureFunctionsConfigurationCache">Azure Functions configuration cache.</param>
        /// <param name="cache">Cache.</param>
        public BartersFetcher(
            ILogger<BartersFetcher> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache,
            ICache cache)
            : base(logger, httpClientWrapperFactory, azureFunctionsConfigurationCache, cache)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<Price>>> DeserializeData(string responseContent)
        {
            List<Price> prices = new();

            JsonElement bartersJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement barterJson in bartersJson.EnumerateArray())
            {
                try
                {
                    List<BarterItem> barterItems = new();
                    string merchant = barterJson.GetProperty("trader").GetProperty("normalizedName").GetString()!;
                    int merchantLevel = barterJson.GetProperty("level").GetInt32();

                    Quest? quest = null;
                    JsonElement taskUnlockJson = barterJson.GetProperty("taskUnlock");

                    if (taskUnlockJson.ValueKind != JsonValueKind.Null)
                    {
                        quest = new Quest()
                        {
                            Id = taskUnlockJson.GetProperty("id").GetString()!,
                            Name = taskUnlockJson.GetProperty("name").GetString()!,
                            WikiLink = taskUnlockJson.GetProperty("wikiLink").GetString()!
                        };
                    }

                    foreach (JsonElement baterItemJson in barterJson.GetProperty("requiredItems").EnumerateArray())
                    {
                        barterItems.Add(new BarterItem()
                        {
                            ItemId = baterItemJson.GetProperty("item").GetProperty("id").GetString()!,
                            Quantity = baterItemJson.GetProperty("quantity").GetInt32()
                        });
                    }

                    foreach (JsonElement itemJson in barterJson.GetProperty("rewardItems").EnumerateArray())
                    {
                        int quantity = itemJson.GetProperty("quantity").GetInt32();
                        Price barter = new()
                        {
                            BarterItems = barterItems.ToArray(),
                            CurrencyName = "barter",
                            ItemId = itemJson.GetProperty("item").GetProperty("id").GetString()!,
                            Merchant = merchant,
                            MerchantLevel = merchantLevel,
                            Quest = quest
                        };

                        if (quantity > 1)
                        {
                            foreach (BarterItem barterItem in barter.BarterItems)
                            {
                                // Dividing the required number of items by the amount of received items
                                barterItem.Quantity /= quantity;
                            }
                        }

                        prices.Add(barter);
                    }
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.BarterDeserializationError, e);
                    Logger.LogError(error);
                }
            }

            return Task.FromResult(Result.Ok(prices.AsEnumerable()));
        }
    }
}
