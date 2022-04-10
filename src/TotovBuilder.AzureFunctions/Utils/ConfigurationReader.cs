using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents a configuration reader.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConfigurationReader : IConfigurationReader
    {
        public static string ApiBarterQueryKey = "TOTOVBUILDER_ApiBarterQuery";
        public static string ApiPriceQueryKey = "TOTOVBUILDER_ApiPriceQuery";
        public static string ApiUrlKey = "TOTOVBUILDER_ApiUrl";
        public static string AzureBlobStorageConnectionStringKey = "TOTOVBUILDER_AzureBlobStorageConnectionString";
        public static string AzureBlobStorageContainerNameKey = "TOTOVBUILDER_AzureBlobStorageContainerName";
        public static string MarketDataCacheDurationKey = "TOTOVBUILDER_MarketDataCacheDuration";
        public static string FetchTimeoutKey = "TOTOVBUILDER_FetchTimeout";
        public static string ItemCategoriesAzureBlobNameKey = "TOTOVBUILDER_ItemCategoriesAzureBlobName";
        public static string ItemsAzureBlobNameKey = "TOTOVBUILDER_ItemsAzureBlobName";
        public static string PresetsAzureBlobNameKey = "TOTOVBUILDER_PresetsAzureBlobName";

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
