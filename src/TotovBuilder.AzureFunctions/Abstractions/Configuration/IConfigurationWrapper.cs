using TotovBuilder.AzureFunctions.Configuration;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions.Configuration
{
    /// <summary>
    /// Provides the functionalities of a wrapper for the configuration of the application.
    /// Used to avoid circular dependencies in <see cref="ConfigurationLoader"/>.
    /// </summary>
    public interface IConfigurationWrapper
    {
        /// <summary>
        /// Azure Functions configuration.
        /// </summary>
        AzureFunctionsConfiguration Values { get; set; }
    }
}
