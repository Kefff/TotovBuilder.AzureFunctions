using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Utils;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an Azure Functions configuration fetcher.
    /// </summary>
    public class AzureFunctionsConfigurationFetcher : RawDataFetcher<AzureFunctionsConfiguration>, IAzureFunctionsConfigurationFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName
        {
            get
            {
                return ConfigurationWrapper.Values.AzureFunctionsConfigurationBlobName;
            }
        }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.AzureFunctionsConfiguration;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsConfigurationFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="configurationLoader">Configuration wrapper.</param>
        public AzureFunctionsConfigurationFetcher(
            ILogger<AzureFunctionsConfigurationFetcher> logger,
            IAzureBlobManager blobDataFetcher,
            IConfigurationWrapper configurationWrapper)
            : base(logger, blobDataFetcher, configurationWrapper)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<AzureFunctionsConfiguration>> DeserializeData(string responseContent)
        {
            AzureFunctionsConfiguration azureFunctionsConfiguration;

            try
            {
                azureFunctionsConfiguration = JsonSerializer.Deserialize<AzureFunctionsConfiguration>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                })!;

                return Task.FromResult(Result.Ok(azureFunctionsConfiguration));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.AzureFunctionsConfigurationDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<AzureFunctionsConfiguration>(error));
            }
        }
    }
}
