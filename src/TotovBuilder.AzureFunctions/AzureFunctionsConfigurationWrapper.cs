using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents a wrapper around the Azure Functions configuration.
    /// </summary>
    public class AzureFunctionsConfigurationWrapper : IAzureFunctionsConfigurationWrapper
    {
        /// <inheritdoc/>
        public Task? LoadingTask { get; private set; } = null;

        /// <inheritdoc/>
        public AzureFunctionsConfiguration Values { get; set; } = new AzureFunctionsConfiguration();

        /// <inheritdoc/>
        public Task EndLoading()
        {
            LoadingTask!.Start();

            return LoadingTask;
        }

        public void StartLoading()
        {
            LoadingTask = new Task(() => { });
        }
    }
}
