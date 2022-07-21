using System.Linq;
using System.Text.Json;
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
        /// <param name="blobFetcher">Blob fetcher.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public ChangelogFetcher(ILogger logger, IBlobFetcher blobFetcher, IConfigurationReader configurationReader, ICache cache)
            : base(logger, blobFetcher, configurationReader, cache)
        {
            _azureBlobName = ConfigurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.AzureChangelogBlobNameKey);
        }
        
        /// <inheritdoc/>
        protected override Result<ChangelogEntry[]> DeserializeData(string responseContent)
        {
            ChangelogEntry[] changelog = JsonSerializer.Deserialize<ChangelogEntry[]>(responseContent, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            return Result.Ok(changelog.OrderByDescending(c => c.Date).ToArray());
        }
    }
}
