using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns values related to Tarkov gameplay to the caller.
    /// </summary>
    public class GetTarkovValues
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
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTarkovValues"/> class.
        /// </summary>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="changelogFetcher">Tarkov values fetcher.</param>
        public GetTarkovValues(IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader,
            IHttpResponseDataFactory httpResponseDataFactory,
            ITarkovValuesFetcher changelogFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
            HttpResponseDataFactory = httpResponseDataFactory;
            TarkovValuesFetcher = changelogFetcher;
        }

        /// <summary>
        /// Gets values related to Tarkov gameplay to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Values related to Tarkov gameplay.</returns>
        [Function("GetTarkovValues")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tarkovvalues")] HttpRequestData httpRequest)
        {
            await AzureFunctionsConfigurationReader.Load();
            TarkovValues tarkovValues = await TarkovValuesFetcher.Fetch() ?? new TarkovValues();

            return await HttpResponseDataFactory.CreateResponse(httpRequest, tarkovValues);
        }
    }
}
