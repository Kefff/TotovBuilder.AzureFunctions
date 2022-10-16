using System.Collections.Generic;
using System.Threading.Tasks;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a wrapper around the Azure Functions configuration.
    /// </summary>
    public interface IAzureFunctionsConfigurationWrapper
    {
        /// <summary>
        /// Fake loading task used to make services dependant on this configuration wait.
        /// </summary>
        Task? LoadingTask { get; }

        /// <summary>
        /// Azure Functions configuration.
        /// </summary>
        AzureFunctionsConfiguration Values { get; set; }

        /// <summary>
        /// Marks the Azure Functions configuration as loaded.
        /// </summary>
        Task EndLoading();

        /// <summary>
        /// Marks the Azure Functions configuration as being loaded.
        /// </summary>
        void StartLoading();
    }
}
