using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Utils;
using static TotovBuilder.AzureFunctions.Utils.JsonDeserializer;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a barters fetcher.
    /// </summary>
    public class BartersFetcher : ApiFetcher<IEnumerable<Price>>, IBartersFetcher
    {
        /// <summary>
        ///  Game mode for which barters are fetched from the API.
        /// </summary>
        public GameMode GameMode { get; init; }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Barters;
            }
        }

        /// <summary>
        /// API requester.
        /// </summary>
        private readonly IApiRequester ApiRequester;

        /// <summary>
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Quests fetcher.
        /// </summary>
        private readonly IQuestsFetcher QuestsFetcher;

        /// <summary>
        /// Traders fetcher.
        /// </summary>
        private readonly ITradersFetcher TradersFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="BartersFetcher"/> class.
        /// </summary>
        /// <param name="gameMode">Game mode for which barters are fetched.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="apiRequester">API requester.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="questsFetcher">Quests fetcher.</param>
        /// <param name="tradersFetcher">Traders fetcher.</param>
        public BartersFetcher(
            GameMode gameMode,
            ILogger<BartersFetcher> logger,
            IApiRequester apiRequester,
            IConfigurationWrapper configurationWrapper,
            IQuestsFetcher questsFetcher,
            ITradersFetcher tradersFetcher)
            : base(logger)
        {
            ApiRequester = apiRequester;
            ConfigurationWrapper = configurationWrapper;
            GameMode = gameMode;
            QuestsFetcher = questsFetcher;
            TradersFetcher = tradersFetcher;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IEnumerable<Price>>> ExecuteFetch()
        {
            string bartersPath = BuildApiPath(ConfigurationWrapper.Values.ApiBartersPath, GameMode.ApiQueryValue);
            Task<Result<JsonElement>> bartersDataTask = ApiRequester.RequestData(bartersPath);
            Task<Result> questsTask = QuestsFetcher.Fetch();
            Task<Result> tradersTask = TradersFetcher.Fetch();

            await Task.WhenAll(bartersDataTask, questsTask, tradersTask);

            Result allTasksResult = Result.Merge(bartersDataTask.Result.ToResult(), questsTask.Result, tradersTask.Result);

            if (allTasksResult.IsFailed)
            {
                return allTasksResult;
            }

            JsonElement bartersJson = bartersDataTask.Result.Value;

            if (bartersJson.ValueKind != JsonValueKind.Array)
            {
                string error = string.Format(Properties.Resources.InvalidApiResponseData, bartersPath, bartersJson.GetRawText());
                Logger.LogError(error);

                return Result.Fail(error);
            }

            List<Price> barters = [];

            foreach (JsonElement barterJson in bartersJson.EnumerateArray())
            {
                try
                {
                    barters.Add(DeserializeBarter(barterJson));
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.BarterDeserializationError, e);
                    Logger.LogError(error);
                }
            }

            // Ignoring barters that require the same item as the one obtained to avoid price calculation infinite loops
            barters.RemoveAll(b => b.BarterItems.Any(bi => bi.ItemId == b.ItemId));

            return Result.Ok(barters.AsEnumerable());
        }

        /// <summary>
        /// Deserializes a barter.
        /// </summary>
        /// <param name="barterJson">Json element representing the barter.</param>
        /// <returns>Barter.</returns>
        private Price DeserializeBarter(JsonElement barterJson)
        {
            List<BarterItem> barterItems = [];

            foreach (JsonElement barterItemJson in barterJson.GetProperty("requiredItems").EnumerateArray())
            {
                barterItems.Add(new BarterItem()
                {
                    ItemId = barterItemJson.GetProperty("item").GetString()!,
                    Quantity = barterItemJson.GetProperty("count").GetDouble()
                });
            }

            JsonElement offeredItemJson = barterJson.GetProperty("offeredItem");
            double quantity = offeredItemJson.GetProperty("count").GetDouble();

            if (quantity != 1)
            {
                foreach (BarterItem barterItem in barterItems)
                {
                    // Dividing the required number of items by the amount of received items
                    barterItem.Quantity /= quantity;
                }
            }

            Price barter = new()
            {
                BarterItems = [.. barterItems],
                CurrencyName = "barter",
                ItemId = offeredItemJson.GetProperty("item").GetString()!,
                Merchant = TradersFetcher.GetMerchantName(barterJson.GetProperty("trader").GetString()!),
                MerchantLevel = barterJson.GetProperty("minTraderLevel").GetInt32()
            };

            if (TryDeserializeString(barterJson, "taskUnlock", out string taskId))
            {
                barter.Quest = QuestsFetcher.GetQuest(taskId);
            }

            return barter;
        }
    }
}
