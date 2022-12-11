using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of an Azure Functions configuration cache.
    /// Used to avoid circular dependencies in <see cref="AzureFunctionsConfigurationReader"/>.
    /// </summary>
    public interface IAzureFunctionsConfigurationCache
    {
        /// <summary>
        /// Azure Functions configuration.
        /// </summary>
        AzureFunctionsConfiguration Values { get; set; }
    }
}
