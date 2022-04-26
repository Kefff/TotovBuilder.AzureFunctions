using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TotovBuilder.AzureFunctions.Abstraction;

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents cache that stores the fetched data.
    /// </summary>
    public class Cache : ICache
    {
        /// <summary>
        /// Policy for caching items.
        /// </summary>
        private readonly MemoryCacheEntryOptions CachingOptions = new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove };

        /// <summary>
        /// Configuration reader;
        /// </summary>
        protected readonly IConfigurationReader ConfigurationReader;

        /// <summary>
        /// Cache instance.
        /// </summary>
        private readonly MemoryCache Instance = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger Logger;

        /// <summary>
        /// Last storage date for each data types.
        /// </summary>
        private readonly Dictionary<DataType, DateTime> LastStorageDates = new Dictionary<DataType, DateTime>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        public Cache(ILogger logger, IConfigurationReader configurationReader)
        {
            ConfigurationReader = configurationReader;
            Logger = logger;
        }
 
        /// <inheritdoc/>
        public string Get(DataType dataType)
        {
            Instance.TryGetValue(dataType.ToString(), out string? value);

            if (string.IsNullOrEmpty(value))
            {
                Logger.LogError(string.Format(Properties.Resources.InvalidCache, dataType));
            }
            
            return value ?? string.Empty;
        }
        
        /// <inheritdoc/>
        public bool HasValidCache(DataType dataType)
        {
            if (!LastStorageDates.TryGetValue(dataType, out DateTime lastStorageDate))
            {
                return false;
            }
            
            TimeSpan durationSinceLastFetch = DateTime.Now - lastStorageDate;
            int cacheDuration = ConfigurationReader.ReadInt(dataType == DataType.Prices ? TotovBuilder.AzureFunctions.ConfigurationReader.PriceCacheDurationKey : TotovBuilder.AzureFunctions.ConfigurationReader.CacheDurationKey);

            return durationSinceLastFetch.TotalSeconds <= cacheDuration;
        }

        /// <inheritdoc/>
        public void Remove(DataType dataType)
        {
            Instance.Remove(dataType.ToString());
            LastStorageDates.Remove(dataType);
        }

        /// <inheritdoc/>
        public void Store(DataType dataType, string data)
        {
            Instance.Set(dataType.ToString(), data, CachingOptions);
            LastStorageDates[dataType] = DateTime.Now;
        }
    }
}
