using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of an Azure functions configuration reader.
    /// </summary>
    public interface IAzureFunctionsConfigurationReader
    {
        /// <summary>
        /// Azure Functions configuration.
        /// </summary>
        AzureFunctionsConfiguration Values { get; set; }

        /// <summary>
        /// Loads the Azure Functions configuration.
        /// </summary>
        Task Load();
    }
}
