using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns prices to the caller.
    /// </summary>
    public class GetPrices
    {
        /// <summary>
        /// Configuration loader.
        /// </summary>
        private readonly IConfigurationLoader ConfigurationLoader;

        /// <summary>
        /// Barters fetcher.
        /// </summary>
        private readonly IBartersFetcher BartersFetcher;

        /// <summary>
        /// HTTP response data factory.
        /// </summary>
        private readonly IHttpResponseDataFactory HttpResponseDataFactory;

        /// <summary>
        /// Prices fetcher.
        /// </summary>
        private readonly IPricesFetcher PricesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPrices"/> class.
        /// </summary>
        /// <param name="configurationLoader">Configuration loader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="bartersFetcher">Barters fetcher.</param>
        /// <param name="pricesFetcher">Prices fetcher.</param>
        public GetPrices(IConfigurationLoader configurationLoader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IBartersFetcher bartersFetcher,
            IPricesFetcher pricesFetcher)
        {
            ConfigurationLoader = configurationLoader;
            HttpResponseDataFactory = httpResponseDataFactory;
            BartersFetcher = bartersFetcher;
            PricesFetcher = pricesFetcher;
        }

        /// <summary>
        /// Gets the prices to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Prices.</returns>
        [Function("GetPrices")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "prices")] HttpRequestData httpRequest)
        {
            await ConfigurationLoader.Load();
            IEnumerable<Price> prices = await PricesFetcher.Fetch();
            IEnumerable<Price> barters = await BartersFetcher.Fetch();

            return await HttpResponseDataFactory.CreateEnumerableResponse(httpRequest, prices.Concat(barters).OrderBy(p => p.ItemId));
        }
    }
}
