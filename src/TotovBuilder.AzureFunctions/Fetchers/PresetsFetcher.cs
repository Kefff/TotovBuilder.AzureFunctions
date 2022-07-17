//using System;
//using FluentResults;
//using Microsoft.Extensions.Logging;
//using TotovBuilder.AzureFunctions.Abstraction;
//using TotovBuilder.AzureFunctions.Abstraction.Fetchers;

//namespace TotovBuilder.AzureFunctions.Fetchers
//{
//    /// <summary>
//    /// Represents a presets fetcher.
//    /// </summary>
//    public class PresetsFetcher : StaticDataFetcher<InventoryItem[]>, IPresetsFetcher
//    {
//        /// <inheritdoc/>
//        protected override DataType DataType => DataType.Presets;

//        /// <inheritdoc/>
//        protected override string AzureBlobName => _azureBlobName;
//        private readonly string _azureBlobName;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="PresetsFetcher"/> class.
//        /// </summary>
//        /// <param name="logger">Logger.</param>
//        /// <param name="blobDataFetcher">Blob data fetcher.</param>
//        /// <param name="configurationReader">Configuration reader.</param>
//        /// <param name="cache">Cache.</param>
//        public PresetsFetcher(ILogger logger, IBlobFetcher blobDataFetcher, IConfigurationReader configurationReader, ICache cache)
//            : base(logger, blobDataFetcher, configurationReader, cache)
//        {
//            _azureBlobName = ConfigurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.AzurePresetsBlobNameKey);
//        }

//        /// <inheritdoc/>
//        protected override Result<InventoryItem[]> GetData(string responseContent)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
