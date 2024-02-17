using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a changelog fetcher.
    /// </summary>
    public class ChangelogFetcher : RawDataFetcher<IEnumerable<ChangelogEntry>>, IChangelogFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName
        {
            get
            {
                return ConfigurationWrapper.Values.RawChangelogBlobName;
            }
        }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Changelog;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureBlobStorageManager">Azure blob storage manager.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public ChangelogFetcher(
            ILogger<ChangelogFetcher> logger,
            IAzureBlobStorageManager azureBlobStorageManager,
            IConfigurationWrapper configurationWrapper)
            : base(logger, azureBlobStorageManager, configurationWrapper)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<ChangelogEntry>>> DeserializeData(string responseContent)
        {
            try
            {
                IEnumerable<ChangelogEntry> changelog = JsonSerializer.Deserialize<IEnumerable<ChangelogEntry>>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                })!;

                return Task.FromResult(Result.Ok(changelog.OrderByDescending(c => c.Date).Take(5).AsEnumerable()));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ChangelogDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<IEnumerable<ChangelogEntry>>(error));
            }
        }
    }
}
