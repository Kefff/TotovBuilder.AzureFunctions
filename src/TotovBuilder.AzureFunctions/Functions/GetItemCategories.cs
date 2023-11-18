using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns item categories to the caller.
    /// </summary>
    public class GetItemCategories
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
        /// Item categories fetcher.
        /// </summary>
        private readonly IItemCategoriesFetcher ItemCategoriesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetItemCategories"/> class.
        /// </summary>
        /// <param name="configurationLoader">Configuration loader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="itemCategoriesFetcher">Item categories fetcher.</param>
        public GetItemCategories(
            IConfigurationLoader configurationLoader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IItemCategoriesFetcher itemCategoriesFetcher)
        {
            ConfigurationLoader = configurationLoader;
            HttpResponseDataFactory = httpResponseDataFactory;
            ItemCategoriesFetcher = itemCategoriesFetcher;
        }

        /// <summary>
        /// Gets the item categories to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Item categories.</returns>
        [Function("GetItemCategories")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "itemcategories")] HttpRequestData httpRequest)
        {
            await ConfigurationLoader.Load();
            IEnumerable<string> itemCategories = (await ItemCategoriesFetcher.Fetch()).Select(c => c.Id);

            return await HttpResponseDataFactory.CreateEnumerableResponse(httpRequest, itemCategories);
        }
    }
}
