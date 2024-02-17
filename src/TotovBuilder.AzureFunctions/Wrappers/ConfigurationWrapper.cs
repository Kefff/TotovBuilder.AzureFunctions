using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Wrappers
{
    /// <summary>
    /// Represents a wrapper for the configuration of the application.
    /// </summary>
    public class ConfigurationWrapper : IConfigurationWrapper
    {
        /// <inheritdoc/>
        public AzureFunctionsConfiguration Values { get; set; } = new AzureFunctionsConfiguration();
    }
}
