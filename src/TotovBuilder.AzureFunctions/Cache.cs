﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TotovBuilder.AzureFunctions.Abstractions;

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
        /// Azure functions onfiguration wrapper;
        /// </summary>
        protected readonly IAzureFunctionsConfigurationWrapper AzureFunctionsConfigurationWrapper;

        /// <summary>
        /// Cache instance.
        /// </summary>
        private readonly MemoryCache Instance = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<Cache> Logger;

        /// <summary>
        /// Last storage date for each data types.
        /// </summary>
        private readonly Dictionary<DataType, DateTime> LastStorageDates = new Dictionary<DataType, DateTime>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configurationReader">Azure Functions configuration wrapper.</param>
        public Cache(ILogger<Cache> logger, IAzureFunctionsConfigurationWrapper azureFunctionsConfigurationWrapper)
        {
            AzureFunctionsConfigurationWrapper = azureFunctionsConfigurationWrapper ;
            Logger = logger;
        }
 
        /// <inheritdoc/>
        public T? Get<T>(DataType dataType)
            where T : class
        {
            if (!Instance.TryGetValue(dataType.ToString(), out T? value))
            {
                Logger.LogError(string.Format(Properties.Resources.InvalidCache, dataType));
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
            int cacheDuration = dataType == DataType.Prices ? AzureFunctionsConfigurationWrapper.Values.PriceCacheDuration : AzureFunctionsConfigurationWrapper.Values.CacheDuration;

            return durationSinceLastFetch.TotalSeconds <= cacheDuration;
        }

        /// <inheritdoc/>
        public void Remove(DataType dataType)
        {
            Instance.Remove(dataType.ToString());
            LastStorageDates.Remove(dataType);
        }

        /// <inheritdoc/>
        public void Store<T>(DataType dataType, T data)
            where T : class
        {
            Instance.Set(dataType.ToString(), data, CachingOptions);
            LastStorageDates[dataType] = DateTime.Now;
        }
    }
}
