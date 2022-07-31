using System.Threading.Tasks;

namespace TotovBuilder.AzureFunctions.Abstraction
{
    /// <summary>
    /// Provides the functionalities of an Azure functions configuration reader.
    /// </summary>
    public interface IAzureFunctionsConfigurationReader
    {
        /// <summary>
        /// Configuration values.
        /// </summary>
        AzureFunctionsConfiguration Values { get; }

        /// <summary>
        /// Waits for the configuration to be loaded.
        /// </summary>
        Task WaitUntilReady();
    }
}
