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
    /// Represents of a Tarkov values fetcher.
    /// </summary>
    public class TarkovValuesFetcher : RawDataFetcher<TarkovValues>, ITarkovValuesFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName
        {
            get
            {
                return ConfigurationWrapper.Values.RawTarkovValuesBlobName;
            }
        }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.TarkovValues;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureBlobStorageManager">Azure blob storage manager.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public TarkovValuesFetcher(
            ILogger<TarkovValuesFetcher> logger,
            IAzureBlobStorageManager azureBlobStorageManager,
            IConfigurationWrapper configurationWrapper)
            : base(logger, azureBlobStorageManager, configurationWrapper)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<TarkovValues>> DeserializeData(string responseContent)
        {
            try
            {
                TarkovValues tarkovValues = JsonSerializer.Deserialize<TarkovValues>(responseContent, SerializationOptions)!;

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
