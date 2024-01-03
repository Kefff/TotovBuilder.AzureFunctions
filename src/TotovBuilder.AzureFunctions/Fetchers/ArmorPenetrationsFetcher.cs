using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a changelog fetcher.
    /// </summary>
    public class ArmorPenetrationsFetcher : RawDataFetcher<IEnumerable<ArmorPenetration>>, IArmorPenetrationsFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName
        {
            get
            {
                return ConfigurationWrapper.Values.RawArmorPenetrationsBlobName;
            }
        }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.ArmorPenetrations;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureBlobStorageManager">Azure blob storage manager.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public ArmorPenetrationsFetcher(
            ILogger<ArmorPenetrationsFetcher> logger,
            IAzureBlobStorageManager azureBlobStorageManager,
            IConfigurationWrapper configurationWrapper)
            : base(logger, azureBlobStorageManager, configurationWrapper)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<ArmorPenetration>>> DeserializeData(string responseContent)
        {
            try
            {
                IEnumerable<ArmorPenetration> armorPenetrations = JsonSerializer.Deserialize<IEnumerable<ArmorPenetration>>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                })!;

                return Task.FromResult(Result.Ok(armorPenetrations.AsEnumerable()));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ArmorPenetrationsDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<IEnumerable<ArmorPenetration>>(error));
            }
        }
    }
}
