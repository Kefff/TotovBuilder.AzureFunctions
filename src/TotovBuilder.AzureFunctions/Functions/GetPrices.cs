using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns prices to the caller.
    /// </summary>
    public class GetPrices
    {
        /// <summary>
        /// Azure Functions configuration reader.
        /// </summary>
        private readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

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
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="bartersFetcher">Barters fetcher.</param>
        /// <param name="pricesFetcher">Prices fetcher.</param>
        public GetPrices(IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IBartersFetcher bartersFetcher,
            IPricesFetcher pricesFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
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
            await AzureFunctionsConfigurationReader.Load();
            IEnumerable<Price> prices = await PricesFetcher.Fetch();
            IEnumerable<Price> barters = await BartersFetcher.Fetch();

            return await HttpResponseDataFactory.CreateEnumerableResponse(httpRequest, prices.Concat(barters).OrderBy(p => p.ItemId));
        }
    }
}
