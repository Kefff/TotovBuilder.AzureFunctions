using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a website configuration fetcher.
    /// </summary>
    public class WebsiteConfigurationFetcher : StaticDataFetcher<WebsiteConfiguration>, IWebsiteConfigurationFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName => AzureFunctionsConfigurationCache.Values.AzureWebsiteConfigurationBlobName;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.WebsiteConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteConfigurationFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="azureFunctionsConfigurationCache">Azure Functions configuration cache.</param>
        /// <param name="cache">Cache.</param>
        public WebsiteConfigurationFetcher(
            ILogger<WebsiteConfigurationFetcher> logger,
            IBlobFetcher blobDataFetcher,
            IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache,
            ICache cache)
            : base(logger, blobDataFetcher, azureFunctionsConfigurationCache, cache)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<WebsiteConfiguration>> DeserializeData(string responseContent)
        {
            WebsiteConfiguration azureFunctionsConfiguration;

            try
            {
                azureFunctionsConfiguration = JsonSerializer.Deserialize<WebsiteConfiguration>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.WebsiteConfigurationDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<WebsiteConfiguration>(error));
            }

            return Task.FromResult(Result.Ok(azureFunctionsConfiguration));
        }
    }
}
