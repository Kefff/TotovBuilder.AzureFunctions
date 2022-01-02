using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents cache that stores the fetched data.
    /// </summary>
    public class Cache : ICache
    {
        private const string ItemCategoriesKey = "itemcategories";
        private const string ItemsKey = "items";
        private const string MarketDataKey = "market";
        private const string PresetsKey = "presets";

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public Cache(ILogger logger)
        {
            Logger = logger;
        }

        /// <inheritdoc/>
        public bool HasStaticDataCached { get; private set; } = false;

        /// <inheritdoc/>
        public DateTime LastMarketDataFetchDate { get; private set; } = new DateTime();

        /// <summary>
        /// Cache instance.
        /// </summary>
        private readonly MemoryCache Instance = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        /// <summary>
        /// Policy for caching items.
        /// </summary>
        private readonly MemoryCacheEntryOptions CachingOptions = new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove };
 
        /// <inheritdoc/>
        public string Get(DataType dataType)
        {
            string? value = null;

            switch (dataType)
            {
                case DataType.ItemCategories:
                    Instance.TryGetValue(ItemCategoriesKey, out value);
                    break;
                case DataType.Items:
                    Instance.TryGetValue(ItemsKey, out value);
                    break;
                case DataType.MarketData:
                    Instance.TryGetValue(MarketDataKey, out value);
                    break;
                case DataType.Presets:
                    Instance.TryGetValue(PresetsKey, out value);
                    break;
            };

            if (string.IsNullOrEmpty(value))
            {
                Logger.LogError(string.Format(Properties.Resources.InvalidCache, dataType));

                return "[]";
            }
            
            return value;
        }

        /// <inheritdoc/>
        public void Remove(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.ItemCategories:
                    Instance.Remove(ItemCategoriesKey);
                    break;
                case DataType.Items:
                    Instance.Remove(ItemsKey);
                    break;
                case DataType.MarketData:
                    Instance.Remove(MarketDataKey);
                    break;
                case DataType.Presets:
                    Instance.Remove(PresetsKey);
                    break;
            }

            if (dataType != DataType.MarketData)
            {
                HasStaticDataCached = false;
            }
        }

        /// <inheritdoc/>
        public void Store(DataType dataType, string data)
        {
            switch (dataType)
            {
                case DataType.ItemCategories:
                    Instance.Set(ItemCategoriesKey, data, CachingOptions);
                    break;
                case DataType.Items:
                    Instance.Set(ItemsKey, data, CachingOptions);
                    break;
                case DataType.MarketData:
                    Instance.Set(MarketDataKey, data, CachingOptions);
                    LastMarketDataFetchDate = DateTime.Now;
                    break;
                case DataType.Presets:
                    Instance.Set(PresetsKey, data, CachingOptions);
                    break;
            }

            if (!HasStaticDataCached
                && Instance.TryGetValue(ItemCategoriesKey, out _)
                && Instance.TryGetValue(ItemsKey, out _)
                && Instance.TryGetValue(PresetsKey, out _))
            {
                HasStaticDataCached = true;
            }
        }
    }
}
