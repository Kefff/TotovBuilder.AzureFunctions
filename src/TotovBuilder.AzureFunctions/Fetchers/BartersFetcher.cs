using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Utils;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a barters fetcher.
    /// </summary>
    public class BartersFetcher : ApiFetcher<IEnumerable<Price>>, IBartersFetcher
    {
        /// <inheritdoc/>
        protected override string ApiQuery
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_apiQuery))
                {
                    _apiQuery = ConfigurationWrapper.Values.ApiBartersQuery.Replace("{0}", GameMode.ApiQueryValue).Replace("{1}", Language);
                }

                return _apiQuery;
            }
        }
        private string? _apiQuery;

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Barters;
            }
        }

        /// <summary>
        /// Game mode for the API request.
        /// </summary>
        public GameMode GameMode { get; init; }

        /// <summary>
        /// Language for the API request.
        /// </summary>
        public string Language { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BartersFetcher"/> class.
        /// </summary>
        /// <param name="gameMode">Game mode for the API request.</param>
        /// <param name="language">Language mode for the API request.param>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public BartersFetcher(
            GameMode gameMode,
            string language,
            ILogger<BartersFetcher> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationWrapper configurationWrapper)
            : base(logger, httpClientWrapperFactory, configurationWrapper)
        {
            GameMode = gameMode;
            Language = language;
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<Price>>> DeserializeData(string responseContent)
        {
            List<Price> barters = [];

            JsonElement bartersJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement barterJson in bartersJson.EnumerateArray())
            {
                try
                {
                    List<BarterItem> barterItems = [];
                    string merchant = barterJson.GetProperty("trader").GetProperty("normalizedName").GetString()!;
                    int merchantLevel = barterJson.GetProperty("level").GetInt32();

                    Quest? quest = null;

                    if (TryDeserializeObject(barterJson, "taskUnlock", out JsonElement taskUnlockJson))
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
                            BarterItems = [.. barterItems],
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

                        barters.Add(barter);
                    }
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.BarterDeserializationError, e);
                    Logger.LogError(error);
                }
            }

            // Ignoring barters that require the same item as the one obtained to avoid price calculation infinite loops
            barters.RemoveAll(b => b.BarterItems.Any(bi => bi.ItemId == b.ItemId));

            return Task.FromResult(Result.Ok(barters.AsEnumerable()));
        }
    }
}
