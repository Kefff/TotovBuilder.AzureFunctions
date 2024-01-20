using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a website configuration fetcher.
    /// </summary>
    public class WebsiteConfigurationFetcher : RawDataFetcher<WebsiteConfiguration>, IWebsiteConfigurationFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName
        {
            get
            {
                return ConfigurationWrapper.Values.RawWebsiteConfigurationBlobName;
            }
        }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.WebsiteConfiguration;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteConfigurationFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureBlobStorageManager">Azure blob storage manager.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="cache">Cache.</param>
        public WebsiteConfigurationFetcher(
            ILogger<WebsiteConfigurationFetcher> logger,
            IAzureBlobStorageManager azureBlobStorageManager,
            IConfigurationWrapper configurationWrapper)
            : base(logger, azureBlobStorageManager, configurationWrapper)
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

                return Task.FromResult(Result.Ok(azureFunctionsConfiguration));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.WebsiteConfigurationDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<WebsiteConfiguration>(error));
            }
        }
    }
}
