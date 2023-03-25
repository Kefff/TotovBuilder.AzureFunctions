using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns item categories to the caller.
    /// </summary>
    public class GetItemCategories
    {
        /// <summary>
        /// Azure Functions configuration reader.
        /// </summary>
        private readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

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
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="itemCategoriesFetcher">Item categories fetcher.</param>
        public GetItemCategories(
            IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IItemCategoriesFetcher itemCategoriesFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
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
            await AzureFunctionsConfigurationReader.Load();
            IEnumerable<string> itemCategories = (await ItemCategoriesFetcher.Fetch())?.Select(c => c.Id) ?? Array.Empty<string>();

            return await HttpResponseDataFactory.CreateEnumerableResponse(httpRequest, itemCategories);
        }
    }
}
