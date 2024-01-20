using FluentResults;

namespace TotovBuilder.AzureFunctions.Abstractions.Utils
{
    /// <summary>
    /// Provides the functionalities of a loader for the configuration of the application.
    /// </summary>
    public interface IConfigurationLoader
    {
        /// <summary>
        /// Waits for the configuration to be loaded.
        /// </summary>
        Task<Result> WaitForLoading();
    }
}
