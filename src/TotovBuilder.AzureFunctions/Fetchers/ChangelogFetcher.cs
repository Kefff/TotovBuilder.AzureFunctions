﻿using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a changelog fetcher.
    /// </summary>
    public class ChangelogFetcher : StaticDataFetcher<IEnumerable<ChangelogEntry>>, IChangelogFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName => AzureFunctionsConfigurationCache.Values.AzureChangelogBlobName;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.Changelog;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobFetcher">Blob fetcher.</param>
        /// <param name="azureFunctionsConfigurationCache">Azure Functions configuration cache.</param>
        /// <param name="cache">Cache.</param>
        public ChangelogFetcher(
            ILogger<ChangelogFetcher> logger,
            IBlobFetcher blobFetcher,
            IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache,
            ICache cache)
            : base(logger, blobFetcher, azureFunctionsConfigurationCache, cache)
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
