﻿namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a cache.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets data from the cache.
        /// </summary>
        /// <param name="dataType">Type of data to retrieve.</param>
        /// <returns>Retrieved data.</returns>
        /// <typeparam name="T">Type of the data to get.</typeparam>
        T? Get<T>(DataType dataType)
            where T : class;

        /// <summary>
        /// Indicates whether the cache is valid for a data type.
        /// </summary>
        /// <param name="dataType">Data type.</param>
        /// <returns><c>true</c> when the cache is valid; otherwise <c>false</c>.</returns>
        bool HasValidCache(DataType dataType);

        /// <summary>
        /// Stores data in the cache.
        /// </summary>
        /// <param name="dataType">Type of data to store.</param>
        /// <param name="data">Data to store.</param>
        void Store<T>(DataType dataType, T data)
            where T : class;

        /// <summary>
        /// Removes data from the cache.
        /// </summary>
        /// <param name="dataType">Type of data to remove.</param>
        void Remove(DataType dataType);
    }
}