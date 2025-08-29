using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Utils;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a fetcher that fetches localized prices for a game mode.
    /// </summary>
    public class GameModeLocalizedPricesFetcher : IGameModeLocalizedPricesFetcher
    {
        private readonly ILogger<PricesFetcher> Logger;
        private readonly IHttpClientWrapperFactory HttpClientWrapperFactory;
        private readonly IConfigurationWrapper ConfigurationWrapper;
        private readonly IBartersFetcher BartersFetcher;
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <inheritdoc/>
        public IEnumerable<GameModeLocalizedPrices>? FetchedData { get; private set; } = null;

        public GameModeLocalizedPricesFetcher(
            ILogger<PricesFetcher> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationWrapper configurationWrapper,
            IBartersFetcher bartersFetcher,
            ITarkovValuesFetcher tarkovValuesFetcher)
        {
            Logger = logger;
            HttpClientWrapperFactory = httpClientWrapperFactory;
            ConfigurationWrapper = configurationWrapper;
            BartersFetcher = bartersFetcher;
            TarkovValuesFetcher = tarkovValuesFetcher;
        }

        /// <inheritdoc/>
        public Task<Result> Fetch()
        {
            if (FetchedData != null)
            {
                return Task.FromResult(Result.Ok());
            }

            List<PricesFetcher> priceFetchers = [];
            List<Task<Result>> fetchTasks = [];

            foreach (GameMode gameMode in ConfigurationWrapper.Values.GameModes)
            {
                foreach (string language in ConfigurationWrapper.Values.Languages)
                {
                    PricesFetcher itemsFetcher = new(
                        gameMode,
                        language,
                        Logger,
                        HttpClientWrapperFactory,
                        ConfigurationWrapper,
                        BartersFetcher,
                        TarkovValuesFetcher);
                    priceFetchers.Add(itemsFetcher);
                    fetchTasks.Add(itemsFetcher.Fetch());
                }
            }

            Task.WaitAll([.. fetchTasks]);
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

            if (fetchedData.Count == 0)
            {
                return Task.FromResult(Result.Fail(string.Format(Properties.Resources.NoDataFetched, DataType.Prices.ToString())));
            }

            FetchedData = fetchedData;

            return Task.FromResult(Result.Ok());
        }
    }
}
