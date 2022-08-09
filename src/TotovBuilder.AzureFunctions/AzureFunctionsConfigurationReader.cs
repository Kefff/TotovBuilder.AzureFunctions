using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents an Azure functions configuration reader.
    /// </summary>
    public class AzureFunctionsConfigurationReader : IAzureFunctionsConfigurationReader
    {
        private const string AzureBlobStorageConnectionStringKey = "TOTOVBUILDER_AzureBlobStorageConnectionString";
        private const string AzureBlobStorageContainerNameKey = "TOTOVBUILDER_AzureBlobStorageContainerName";
        private const string AzureFunctionsConfigurationBlobNameKey = "TOTOVBUILDER_AzureFunctionsConfigurationBlobName";
        
        /// <summary>
        /// Azure Functions configuration fetcher.
        /// </summary>
        private readonly IAzureFunctionsConfigurationFetcher AzureFunctionsConfigurationFetcher;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<AzureFunctionsConfigurationReader> Logger;

        /// <summary>
        /// Wrapper in which the readen Azure Functions configuration is stored.
        /// </summary>
        private readonly IAzureFunctionsConfigurationWrapper AzureFunctionsConfigurationWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsConfigurationInitializer"/> class.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="azureFunctionsConfigurationFetcher">Azure Functions configuration fetcher.</param>
        /// <param name="azureFunctionsConfigurationWrapper">Wrapper in which the readen Azure Functions configuration is stored.</param>
        public AzureFunctionsConfigurationReader(
            ILogger<AzureFunctionsConfigurationReader> logger,
            IAzureFunctionsConfigurationWrapper azureFunctionsConfigurationWrapper,
            IAzureFunctionsConfigurationFetcher azureFunctionsConfigurationFetcher)
        {
            AzureFunctionsConfigurationWrapper = azureFunctionsConfigurationWrapper;
            AzureFunctionsConfigurationFetcher = azureFunctionsConfigurationFetcher;
            Logger = logger;
        }

        /// <summary>
        /// Loads the Azure Functions configuration.
        /// </summary>
        public async Task Load()
        {
            if (AzureFunctionsConfigurationWrapper.IsLoaded())
            {
                return;
            }

            // Temporary configuration for the fetcher to be able to get the configuration blob.
            // Will be replaced by the complete configuration once it is loaded.
            AzureFunctionsConfigurationWrapper.Values = new AzureFunctionsConfiguration()
            {
                AzureBlobStorageConnectionString = ReadString(AzureBlobStorageConnectionStringKey),
                AzureBlobStorageContainerName = ReadString(AzureBlobStorageContainerNameKey),
                AzureFunctionsConfigurationBlobName = ReadString(AzureFunctionsConfigurationBlobNameKey)
            };

            AzureFunctionsConfigurationWrapper.Values = await AzureFunctionsConfigurationFetcher.Fetch() ?? throw new Exception(Properties.Resources.InvalidConfiguration);
            await AzureFunctionsConfigurationWrapper.SetLoaded();
        }

        ///// <summary>
        ///// Reads an integer value from the configuration.
        ///// </summary>
        ///// <param name="key">Key to read.</param>
        ///// <returns>Value if the key is found; otherwise 0.</returns>
        //private int ReadInt(string key)
        //{
        //    string stringValue = ReadString(key);

        //    if (string.IsNullOrWhiteSpace(stringValue))
        //    {
        //        return 0;
        //    }

        //    if (!int.TryParse(stringValue, out int integerValue))
        //    {
        //        Logger.LogError(string.Format(Properties.Resources.InvalidIntegerConfigurationValue, key));
        //    }
            
        //    return integerValue;
        //}
        
        /// <summary>
        /// Reads a string value from the configuration.
        /// </summary>
        /// <param name="key">Key to read.</param>
        /// <returns>Value if the key is found; otherwise an empty string.</returns>
        private string ReadString(string key)
        {
            string? value = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrWhiteSpace(value))
            {
                Logger.LogError(string.Format(Properties.Resources.InvalidStringConfigurationValue, key));
            }
            
            return value ?? string.Empty;
        }
    }
}
