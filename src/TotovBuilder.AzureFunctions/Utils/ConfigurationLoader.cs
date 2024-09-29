using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Utils;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents a loader for the configuration of the application.
    /// </summary>
    public class ConfigurationLoader : IConfigurationLoader
    {
        private const string AzureBlobStorageConnectionStringKey = "TOTOVBUILDER_AzureBlobStorageConnectionString";
        private const string AzureBlobStorageContainerNameKey = "TOTOVBUILDER_AzureBlobStorageContainerName";
        private const string AzureFunctionsConfigurationBlobNameKey = "TOTOVBUILDER_AzureFunctionsConfigurationBlobName";

        /// <summary>
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Azure Functions configuration fetcher.
        /// </summary>
        private readonly IAzureFunctionsConfigurationFetcher AzureFunctionsConfigurationFetcher;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<ConfigurationLoader> Logger;

        /// <summary>
        /// Loading task.
        /// Used to avoid launching multiple loading operations at the same time.
        /// </summary>
        private readonly Task<Result> LoadingTask = Task.FromResult(Result.Ok());

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsConfigurationInitializer"/> class.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="azureFunctionsConfigurationFetcher">Azure Functions configuration fetcher.</param>
        public ConfigurationLoader(
            ILogger<ConfigurationLoader> logger,
            IConfigurationWrapper configurationWrapper,
            IAzureFunctionsConfigurationFetcher azureFunctionsConfigurationFetcher)
        {
            ConfigurationWrapper = configurationWrapper;
            AzureFunctionsConfigurationFetcher = azureFunctionsConfigurationFetcher;
            Logger = logger;

            // Temporary configuration for the fetcher to be able to get the configuration blob.
            // Will be replaced by the complete configuration once it is loaded.
            ConfigurationWrapper.Values = new AzureFunctionsConfiguration()
            {
                AzureBlobStorageConnectionString = ReadString(AzureBlobStorageConnectionStringKey),
                AzureBlobStorageRawDataContainerName = ReadString(AzureBlobStorageContainerNameKey),
                AzureFunctionsConfigurationBlobName = ReadString(AzureFunctionsConfigurationBlobNameKey)
            };

            LoadingTask = Load();
        }

        /// <inheritdoc/>
        public Task<Result> WaitForLoading()
        {
            return LoadingTask;
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        /// <returns>Result of the configuration loading.</returns>
        private async Task<Result> Load()
        {
            Result azureFunctionsConfigurationResult = await AzureFunctionsConfigurationFetcher.Fetch();

            if (azureFunctionsConfigurationResult.IsSuccess)
            {
                ConfigurationWrapper.Values = AzureFunctionsConfigurationFetcher.FetchedData!;
            }

            return azureFunctionsConfigurationResult;
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
