using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Utils;
using static TotovBuilder.AzureFunctions.Utils.JsonDeserializer;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a traders fetcher.
    /// </summary>
    public class TradersFetcher : ApiFetcher<IReadOnlyDictionary<string, string>>, ITradersFetcher
    {
        /// <summary>
        ///  Game mode for which traders are fetched from the API.
        /// </summary>
        public GameMode GameMode { get; init; }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Traders;
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
        /// Initializes a new instance of the <see cref="TradersFetcher"/> class.
        /// </summary>
        /// <param name="gameMode">Game mode for which traders are fetched.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="apiRequester">API requester.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public TradersFetcher(
            GameMode gameMode,
            ILogger<TradersFetcher> logger,
            IApiRequester apiRequester,
            IConfigurationWrapper configurationWrapper)
            : base(logger)
        {
            ApiRequester = apiRequester;
            ConfigurationWrapper = configurationWrapper;
            GameMode = gameMode;
        }

        /// <inheritdoc/>
        public string GetMerchantName(string traderId)
        {
            if (FetchedData == null || !FetchedData.TryGetValue(traderId, out string? merchantName))
            {
                throw new InvalidDataException(string.Format(Properties.Resources.TraderNotFound, traderId));
            }

            return merchantName;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IReadOnlyDictionary<string, string>>> ExecuteFetch()
        {
            string tradersPath = BuildApiPath(ConfigurationWrapper.Values.ApiTradersPath, GameMode.ApiQueryValue);
            Result<JsonElement> tradersResult = await ApiRequester.RequestData(tradersPath);

            if (tradersResult.IsFailed)
            {
                return tradersResult.ToResult();
            }

            JsonElement tradersJson = tradersResult.Value;

            if (tradersJson.ValueKind != JsonValueKind.Object)
            {
                string error = string.Format(Properties.Resources.InvalidApiResponseData, tradersPath, tradersJson.GetRawText());
                Logger.LogError(error);

                return Result.Fail(error);
            }

            Dictionary<string, string> merchantNames = [];

            foreach (JsonProperty traderJson in tradersJson.EnumerateObject())
            {
                if (TryDeserializeString(traderJson.Value, "normalizedName", out string normalizedName))
                {
                    merchantNames[traderJson.Name] = normalizedName;
                }
            }

            return Result.Ok<IReadOnlyDictionary<string, string>>(merchantNames);
        }
    }
}
