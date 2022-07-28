using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a changelog fetcher.
    /// </summary>
    public class ChangelogFetcher : StaticDataFetcher<IEnumerable<ChangelogEntry>>, IChangelogFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobNameKey => TotovBuilder.AzureFunctions.ConfigurationReader.AzureChangelogBlobNameKey;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.Changelog;

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
        }
        
        /// <inheritdoc/>
        protected override Task<IEnumerable<ChangelogEntry>> DeserializeData(string responseContent)
        {
            List<ChangelogEntry> changelog = new List<ChangelogEntry>();

            try
            {
                changelog.AddRange(JsonSerializer.Deserialize<IEnumerable<ChangelogEntry>>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                }));            
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ChangelogDeserializationError, e);
                Logger.LogError(error);
            }

            return Task.FromResult(changelog.OrderByDescending(c => c.Date).AsEnumerable());
        }
    }
}
