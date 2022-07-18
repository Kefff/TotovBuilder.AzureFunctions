using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a changelog fetcher.
    /// </summary>
    public class ChangelogFetcher : StaticDataFetcher<ChangelogEntry[]>, IChangelogFetcher
    {
        /// <inheritdoc/>
        protected override DataType DataType => DataType.Changelog;
        
        /// <inheritdoc/>
        protected override string AzureBlobName => _azureBlobName;
        private readonly string _azureBlobName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public ChangelogFetcher(ILogger logger, IBlobFetcher blobDataFetcher, IConfigurationReader configurationReader, ICache cache)
            : base(logger, blobDataFetcher, configurationReader, cache)
        {
            _azureBlobName = ConfigurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.AzureChangelogBlobNameKey);
        }
        
        /// <inheritdoc/>
        protected override Result<ChangelogEntry[]> GetData(string responseContent)
        {
            throw new System.NotImplementedException();
        }
    }
}
