using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents an Azure Functions configuration cache.
    /// </summary>
    public class AzureFunctionsConfigurationCache : IAzureFunctionsConfigurationCache
    {
        /// <inheritdoc/>
        public AzureFunctionsConfiguration Values { get; set; } = new AzureFunctionsConfiguration();
    }
}
