using System;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a cache.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Indicates whether data that never expire has been cached or not.
        /// </summary>
        bool HasStaticDataCached { get; }

        /// <summary>
        /// Date of the last time data market data was fetched.
        /// </summary>
        DateTime LastMarketDataFetchDate { get; }

        /// <summary>
        /// Gets data from the cache.
        /// </summary>
        /// <param name="dataType">Type of data to retrieve.</param>
        /// <returns>Retrieved data.</returns>
        string Get(DataType dataType);

        /// <summary>
        /// Stores data in the cache.
        /// </summary>
        /// <param name="dataType">Type of data to store.</param>
        /// <param name="data">Data to store.</param>
        void Store(DataType dataType, string data);

        /// <summary>
        /// Removes data from the cache.
        /// </summary>
        /// <param name="dataType">Type of data to remove.</param>
        void Remove(DataType dataType);
    }
}