using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns the changelog to the caller.
    /// </summary>
    public class GetChangelog
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
        /// Changelog fetcher.
        /// </summary>
        private readonly IChangelogFetcher ChangelogFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetChangelog"/> class.
        /// </summary>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="changelogFetcher">Changelog fetcher.</param>
        public GetChangelog(
            IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IChangelogFetcher changelogFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
            HttpResponseDataFactory = httpResponseDataFactory;
            ChangelogFetcher = changelogFetcher;
        }

        /// <summary>
        /// Gets the changelog to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Changelog.</returns>
        [Function("GetChangelog")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "changelog")] HttpRequestData httpRequest)
        {
            await AzureFunctionsConfigurationReader.Load();
            IEnumerable<ChangelogEntry> changelog = await ChangelogFetcher.Fetch() ?? Array.Empty<ChangelogEntry>();

            return await HttpResponseDataFactory.CreateResponse(httpRequest, changelog);
        }
    }
}
