using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;

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
        /// Azure Functions configuration cache.
        /// </summary>
        private readonly IAzureFunctionsConfigurationCache AzureFunctionsConfigurationCache;

        /// <summary>
        /// Azure Functions configuration fetcher.
        /// </summary>
        private readonly IAzureFunctionsConfigurationFetcher AzureFunctionsConfigurationFetcher;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<AzureFunctionsConfigurationReader> Logger;

        /// <summary>
        /// Loading task.
        /// Used to avoid launching multiple loading operations at the same time.
        /// </summary>
        private Task LoadingTask = Task.CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsConfigurationInitializer"/> class.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="azureFunctionsConfigurationFetcher">Azure Functions configuration fetcher.</param>
        public AzureFunctionsConfigurationReader(
            ILogger<AzureFunctionsConfigurationReader> logger,
            IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache,
            IAzureFunctionsConfigurationFetcher azureFunctionsConfigurationFetcher)
        {
            AzureFunctionsConfigurationCache = azureFunctionsConfigurationCache;
            AzureFunctionsConfigurationFetcher = azureFunctionsConfigurationFetcher;
            Logger = logger;

            // Temporary configuration for the fetcher to be able to get the configuration blob.
            // Will be replaced by the complete configuration once it is loaded.
            AzureFunctionsConfigurationCache.Values = new AzureFunctionsConfiguration()
            {
                AzureBlobStorageConnectionString = ReadString(AzureBlobStorageConnectionStringKey),
                AzureBlobStorageContainerName = ReadString(AzureBlobStorageContainerNameKey),
                AzureFunctionsConfigurationBlobName = ReadString(AzureFunctionsConfigurationBlobNameKey)
            };
        }

        /// <summary>
        /// Loads the Azure Functions configuration.
        /// </summary>
        public async Task Load()
        {
            if (!LoadingTask.IsCompleted)
            {
                await LoadingTask;

                return;
            }

            LoadingTask = Task.Run(async () =>
            {
                AzureFunctionsConfigurationCache.Values = await AzureFunctionsConfigurationFetcher.Fetch() ?? throw new Exception(Properties.Resources.InvalidConfiguration);
            });
            await LoadingTask;
        }

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
