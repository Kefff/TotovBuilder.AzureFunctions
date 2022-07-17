using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents a configuration reader.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConfigurationReader : IConfigurationReader
    {
        public static string ApiBartersQueryKey = "TOTOVBUILDER_ApiBartersQuery";
        public static string ApiItemsQueryKey = "TOTOVBUILDER_ApiItemsQuery";
        public static string ApiPricesQueryKey = "TOTOVBUILDER_ApiPricesQuery";
        public static string ApiQuestsQueryKey = "TOTOVBUILDER_ApiQuestsQuery";
        public static string ApiUrlKey = "TOTOVBUILDER_ApiUrl";
        public static string AzureBlobStorageConnectionStringKey = "TOTOVBUILDER_AzureBlobStorageConnectionString";
        public static string AzureBlobStorageContainerNameKey = "TOTOVBUILDER_AzureBlobStorageContainerName";
        public static string AzureItemCategoriesBlobNameKey = "TOTOVBUILDER_AzureItemCategoriesBlobName";
        public static string AzurePresetsBlobNameKey = "TOTOVBUILDER_AzurePresetsBlobName";
        public static string CacheDurationKey = "TOTOVBUILDER_CacheDuration";
        public static string FetchTimeoutKey = "TOTOVBUILDER_FetchTimeout";
        public static string PriceCacheDurationKey = "TOTOVBUILDER_PriceCacheDuration";

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<ConfigurationReader> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationReader"/> class.
        /// </summary>
        /// <param name="logger">Logger</param>
        public ConfigurationReader(ILogger<ConfigurationReader> logger)
        {
            Logger = logger;
        }
        
        /// <inheritdoc/>
        public int ReadInt(string key)
        {
            string stringValue = ReadString(key);

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return 0;
            }

            if (!int.TryParse(stringValue, out int integerValue))
            {
                Logger.LogError(string.Format(Properties.Resources.InvalidIntegerConfigurationValue, key));
            }
            
            return integerValue;
        }

        /// <inheritdoc/>
        public string ReadString(string key)
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
