using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an Azure Functions configuration fetcher.
    /// </summary>
    public class AzureFunctionsConfigurationFetcher : StaticDataFetcher<AzureFunctionsConfiguration>, IAzureFunctionsConfigurationFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName => AzureFunctionsConfigurationReader.Values.AzureFunctionsConfigurationBlobName;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.AzureFunctionsConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsConfigurationFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public AzureFunctionsConfigurationFetcher(ILogger logger, IBlobFetcher blobDataFetcher, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, ICache cache)
            : base(logger, blobDataFetcher, azureFunctionsConfigurationReader, cache)
        {
        }
        
        /// <inheritdoc/>
        protected override Task<AzureFunctionsConfiguration> DeserializeData(string responseContent)
        {
            AzureFunctionsConfiguration azureFunctionsConfiguration = new AzureFunctionsConfiguration();

            try
            {
                azureFunctionsConfiguration = JsonSerializer.Deserialize<AzureFunctionsConfiguration>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.AzureFunctionsConfigurationDeserializationError, e);
                Logger.LogError(error);
            }

            return Task.FromResult(azureFunctionsConfiguration);
        }
    }
}
