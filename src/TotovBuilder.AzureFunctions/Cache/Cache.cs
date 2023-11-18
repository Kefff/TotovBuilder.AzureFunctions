using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TotovBuilder.AzureFunctions.Abstractions.Cache;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;

namespace TotovBuilder.AzureFunctions.Cache
{
    /// <summary>
    /// Represents cache that stores the fetched data.
    /// </summary>
    public class Cache : ICache
    {

        /// <summary>
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Policy for caching items.
        /// </summary>
        private readonly MemoryCacheEntryOptions CachingOptions = new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove };

        /// <summary>
        /// Cache instance.
        /// </summary>
        private readonly MemoryCache Instance = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        /// <summary>
        /// Last storage date for each data types.
        /// </summary>
        private readonly Dictionary<DataType, DateTime> LastStorageDates = new Dictionary<DataType, DateTime>();
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<Cache> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public Cache(ILogger<Cache> logger, IConfigurationWrapper configurationWrapper)
        {
            ConfigurationWrapper = configurationWrapper;
            Logger = logger;
        }

        /// <inheritdoc/>
        public T? Get<T>(DataType dataType)
            where T : class
        {
            if (!Instance.TryGetValue(dataType.ToString(), out T? value))
            {
                Logger.LogError(Properties.Resources.InvalidCache, dataType);
            }

            return value;
        }

        /// <inheritdoc/>
        public bool HasValidCache(DataType dataType)
        {
            if (!LastStorageDates.TryGetValue(dataType, out DateTime lastStorageDate))
            {
                return false;
            }

            TimeSpan durationSinceLastFetch = DateTime.Now - lastStorageDate;
            int cacheDuration = dataType == DataType.Prices
                ? ConfigurationWrapper.Values.PriceCacheDuration
                : ConfigurationWrapper.Values.CacheDuration;

            return durationSinceLastFetch.TotalSeconds <= cacheDuration;
        }

        /// <inheritdoc/>
        public void Remove(DataType dataType)
        {
            Instance.Remove(dataType.ToString());
            LastStorageDates.Remove(dataType);
        }

        /// <inheritdoc/>
        public void Store<T>(DataType dataType, T data, bool updatedStorageDate = true)
            where T : class
        {
            Instance.Set(dataType.ToString(), data, CachingOptions);

            if (updatedStorageDate)
            {
                LastStorageDates[dataType] = DateTime.Now;
            }
        }
    }
}
