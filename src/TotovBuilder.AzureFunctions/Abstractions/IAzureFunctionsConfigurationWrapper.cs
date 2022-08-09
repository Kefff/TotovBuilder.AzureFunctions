using System.Collections.Generic;
using System.Threading.Tasks;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a wrapper around the Azure Functions configuration.
    /// </summary>
    public interface IAzureFunctionsConfigurationWrapper
    {
        /// <summary>
        /// Azure Functions configuration.
        /// </summary>
        AzureFunctionsConfiguration Values { get; set; }

        /// <summary>
        /// Indicates whether the Azure Functions configuration is loaded.
        /// </summary>
        /// <returns><c>true</c> when the Azure Functions configuration is loaded; otherwise <c>false</c>.</returns>
        bool IsLoaded();

        /// <summary>
        /// Marks the Azure Functions configuration as loaded.
        /// </summary>
        Task SetLoaded();
    }
}
