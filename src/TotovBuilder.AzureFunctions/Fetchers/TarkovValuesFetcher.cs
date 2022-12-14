using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents of a Tarkov values fetcher.
    /// </summary>
    public class TarkovValuesFetcher : StaticDataFetcher<TarkovValues>, ITarkovValuesFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName => AzureFunctionsConfigurationCache.Values.AzureTarkovValuesBlobName;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.TarkovValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobFetcher">Blob fetcher.</param>
        /// <param name="azureFunctionsConfigurationCache">Azure Functions configuration cache.</param>
        /// <param name="cache">Cache.</param>
        public TarkovValuesFetcher(
            ILogger<TarkovValuesFetcher> logger,
            IBlobFetcher blobFetcher,
            IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache,
            ICache cache)
            : base(logger, blobFetcher, azureFunctionsConfigurationCache, cache)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<TarkovValues>> DeserializeData(string responseContent)
        {
            try
            {
                TarkovValues tarkovValues = JsonSerializer.Deserialize<TarkovValues>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                })!;

                return Task.FromResult(Result.Ok(tarkovValues));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.TarkovValuesDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<TarkovValues>(error));
            }
        }
    }
}
