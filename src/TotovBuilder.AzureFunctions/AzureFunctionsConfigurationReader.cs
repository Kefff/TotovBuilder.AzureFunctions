using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents an Azure functions configuration reader.
    /// </summary>
    public class AzureFunctionsConfigurationReader : IAzureFunctionsConfigurationReader
    {
        /// <inheritdoc/>
        public AzureFunctionsConfiguration Values { get; private set; } = new AzureFunctionsConfiguration();

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
        /// Fake loading task used to wait util the loading has finished.
        /// </summary>
        private readonly Task LoadingTask = new Task(() => { });

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsConfigurationInitializer"/> class.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="azureFunctionsConfigurationFetcher">Azure Functions configuration fetcher.</param>
        public AzureFunctionsConfigurationReader(ILogger<AzureFunctionsConfigurationReader> logger, IAzureFunctionsConfigurationFetcher azureFunctionsConfigurationFetcher)
        {
            AzureFunctionsConfigurationFetcher = azureFunctionsConfigurationFetcher;
            Logger = logger;

            // Temporary configuration for the fetcher.
            // Will be replaced by the complete configuration once it is loaded.
            AddEnvironmentConfiguration();

            _ = Load();
        }

        /// <inheritdoc/>
        public Task WaitUntilReady()
        {
            return LoadingTask;
        }

        /// <summary>
        /// Adds environment configuration values to the Azure Functions configuration.
        /// </summary>
        private void AddEnvironmentConfiguration()
        {
            Values.AzureBlobStorageConnectionString = ReadString(AzureBlobStorageConnectionStringKey);
            Values.AzureBlobStorageContainerName = ReadString(AzureBlobStorageContainerNameKey);
            Values.AzureFunctionsConfigurationBlobName = ReadString(AzureFunctionsConfigurationBlobNameKey);
        }

        /// <summary>
        /// Loads the Azure Functions configuration.
        /// </summary>
        private async Task Load()
        {
            Values = await AzureFunctionsConfigurationFetcher.Fetch() ?? throw new Exception(Properties.Resources.InvalidConfiguration);
            AddEnvironmentConfiguration();

            LoadingTask.Start();
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
