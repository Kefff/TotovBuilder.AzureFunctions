using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a changelog fetcher.
    /// </summary>
    public class ArmorPenetrationsFetcher : StaticDataFetcher<IEnumerable<ArmorPenetration>>, IArmorPenetrationsFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName => AzureFunctionsConfigurationWrapper.Values.AzureArmorPenetrationsBlobName;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.ArmorPenetrations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobFetcher">Blob fetcher.</param>
        /// <param name="azureFunctionsConfigurationWrapper">Azure Functions configuration wrapper.</param>
        /// <param name="cache">Cache.</param>
        public ArmorPenetrationsFetcher(ILogger<ArmorPenetrationsFetcher> logger, IBlobFetcher blobFetcher, IAzureFunctionsConfigurationWrapper azureFunctionsConfigurationWrapper, ICache cache)
            : base(logger, blobFetcher, azureFunctionsConfigurationWrapper, cache)
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
                });

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
