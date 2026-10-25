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
    /// Represents a fetcher that fetches localized prices for a game mode.
    /// </summary>
    public class GameModeLocalizedPricesFetcher : ApiFetcher<IEnumerable<GameModeLocalizedPrices>>, IGameModeLocalizedPricesFetcher
    {
        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Prices;
            }
        }

        private readonly IApiRequester ApiRequester;
        private readonly ILogger<BartersFetcher> BartersFetcherLogger;
        private readonly IConfigurationWrapper ConfigurationWrapper;
        private readonly ILogger<PricesFetcher> PricesFetcherLogger;
        private readonly ILogger<QuestsFetcher> QuestsFetcherLogger;
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;
        private readonly TimeProvider TimeProvider;
        private readonly ILogger<TradersFetcher> TradersFetcherLogger;

        public GameModeLocalizedPricesFetcher(
            ILogger<PricesFetcher> pricesFetcherLogger,
            ILogger<BartersFetcher> bartersFetcherLogger,
            ILogger<QuestsFetcher> questsFetcherLogger,
            ILogger<TradersFetcher> tradersFetcherLogger,
            IApiRequester apiRequester,
            IConfigurationWrapper configurationWrapper,
            ITarkovValuesFetcher tarkovValuesFetcher,
            TimeProvider timeProvider)
            : base(pricesFetcherLogger)
        {
            ApiRequester = apiRequester;
            BartersFetcherLogger = bartersFetcherLogger;
            ConfigurationWrapper = configurationWrapper;
            PricesFetcherLogger = pricesFetcherLogger;
            QuestsFetcherLogger = questsFetcherLogger;
            TarkovValuesFetcher = tarkovValuesFetcher;
            TimeProvider = timeProvider;
            TradersFetcherLogger = tradersFetcherLogger;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IEnumerable<GameModeLocalizedPrices>>> ExecuteFetch()
        {
            List<PricesFetcher> priceFetchers = [];
            List<Task<Result>> fetchTasks = [];

            foreach (GameMode gameMode in ConfigurationWrapper.Values.GameModes)
            {
                // Temporary game modes are only fetched while they are available; otherwise the prices of their fallback game mode are used
                if (gameMode.FallbackGameModeName != null && !await IsSeasonInProgress())
                {
                    continue;
                }

                // Traders do not depend on the language and are shared by the prices and barters of all languages
                TradersFetcher tradersFetcher = new(
                    gameMode,
                    TradersFetcherLogger,
                    ApiRequester,
                    ConfigurationWrapper);

                foreach (string language in ConfigurationWrapper.Values.ItemsLanguages)
                {
                    QuestsFetcher questsFetcher = new(
                        gameMode,
                        language,
                        QuestsFetcherLogger,
                        ApiRequester,
                        ConfigurationWrapper);
                    BartersFetcher bartersFetcher = new(
                        gameMode,
                        BartersFetcherLogger,
                        ApiRequester,
                        ConfigurationWrapper,
                        questsFetcher,
                        tradersFetcher);
                    PricesFetcher pricesFetcher = new(
                        gameMode,
                        language,
                        PricesFetcherLogger,
                        ApiRequester,
                        ConfigurationWrapper,
                        bartersFetcher,
                        questsFetcher,
                        TarkovValuesFetcher,
                        tradersFetcher);
                    priceFetchers.Add(pricesFetcher);
                    fetchTasks.Add(pricesFetcher.Fetch());
                }
            }

            await Task.WhenAll(fetchTasks);
            List<GameModeLocalizedPrices> fetchedData = [];

            foreach (PricesFetcher priceFetcher in priceFetchers.Where(ife => ife.FetchedData != null))
            {
                fetchedData.Add(new GameModeLocalizedPrices()
                {
                    GameMode = priceFetcher.GameMode,
                    Prices = [.. priceFetcher.FetchedData!],
                    Language = priceFetcher.Language
                });
            }

            AddFallbackPrices(fetchedData);

            if (fetchedData.Count == 0)
            {
                return Result.Fail(string.Format(Properties.Resources.NoDataFetched, DataType.ToString()));
            }

            return Result.Ok(fetchedData.AsEnumerable());
        }

        /// <summary>
        /// Adds the prices of their fallback game mode to the game modes whose prices could not be fetched.
        /// </summary>
        /// <param name="fetchedData">Fetched prices.</param>
        private void AddFallbackPrices(List<GameModeLocalizedPrices> fetchedData)
        {
            foreach (GameMode gameMode in ConfigurationWrapper.Values.GameModes.Where(gm => gm.FallbackGameModeName != null))
            {
                bool isFallbackUsed = false;

                foreach (string language in ConfigurationWrapper.Values.ItemsLanguages)
                {
                    if (fetchedData.Any(fd => fd.GameMode.Name == gameMode.Name && fd.Language == language))
                    {
                        continue;
                    }

                    GameModeLocalizedPrices? fallbackPrices = fetchedData.FirstOrDefault(fd => fd.GameMode.Name == gameMode.FallbackGameModeName && fd.Language == language);

                    if (fallbackPrices == null)
                    {
                        continue;
                    }

                    fetchedData.Add(new GameModeLocalizedPrices()
                    {
                        GameMode = gameMode,
                        Prices = fallbackPrices.Prices,
                        Language = language
                    });
                    isFallbackUsed = true;
                }

                if (isFallbackUsed)
                {
                    Logger.LogInformation(Properties.Resources.GameModeFallbackUsed, gameMode.Name, gameMode.FallbackGameModeName);
                }
            }
        }

        /// <summary>
        /// Indicates whether a season is in progress.
        /// </summary>
        /// <returns><c>true</c> when a season is in progress; otherwise <c>false</c>.</returns>
        private async Task<bool> IsSeasonInProgress()
        {
            Result<JsonElement> seasonResult = await ApiRequester.RequestData(ConfigurationWrapper.Values.ApiSeasonPath);

            if (seasonResult.IsFailed)
            {
                return false;
            }

            long now = TimeProvider.GetUtcNow().ToUnixTimeMilliseconds();
            bool hasStarted = TryDeserializeDouble(seasonResult.Value, "start", out double start) && start <= now;
            bool hasEnded = TryDeserializeDouble(seasonResult.Value, "end", out double end) && end < now;

            return hasStarted && !hasEnded;
        }
    }
}
