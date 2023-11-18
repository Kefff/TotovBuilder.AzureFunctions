using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns items to the caller.
    /// </summary>
    public class GetItems
    {
        /// <summary>
        /// Configuration loader.
        /// </summary>
        private readonly IConfigurationLoader ConfigurationLoader;

        /// <summary>
        /// HTTP response data factory.
        /// </summary>
        private readonly IHttpResponseDataFactory HttpResponseDataFactory;

        /// <summary>
        /// Items fetcher.
        /// </summary>
        private readonly IItemsFetcher ItemsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetItems"/> class.
        /// </summary>
        /// <param name="configurationLoader">Configuration loader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="itemsFetcher">Items fetcher.</param>
        public GetItems(IConfigurationLoader configurationLoader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IItemsFetcher itemsFetcher)
        {
            ConfigurationLoader = configurationLoader;
            HttpResponseDataFactory = httpResponseDataFactory;
            ItemsFetcher = itemsFetcher;
        }

        /// <summary>
        /// Gets the items to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Items.</returns>
        [Function("GetItems")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "items")] HttpRequestData httpRequest)
        {
            await ConfigurationLoader.Load();
            IEnumerable<Item> items = await ItemsFetcher.Fetch();

            return await HttpResponseDataFactory.CreateEnumerableResponse(httpRequest, items);
        }
    }
}
