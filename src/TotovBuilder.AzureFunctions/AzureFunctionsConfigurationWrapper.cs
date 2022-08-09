using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents a wrapper around the Azure Functions configuration.
    /// </summary>
    public class AzureFunctionsConfigurationWrapper : IAzureFunctionsConfigurationWrapper
    {
        /// <inheritdoc/>
        public AzureFunctionsConfiguration Values { get; set; } = new AzureFunctionsConfiguration();

        /// <summary>
        /// Fake loading task used to make services dependant on this configuration wait.
        /// </summary>
        private readonly Task LoadingTask = new Task(() => { });

        /// <summary>
        /// In
        /// </summary>
        /// <returns></returns>
        public bool IsLoaded()
        {
            return LoadingTask.IsCompleted;
        }

        /// <inheritdoc/>
        public Task SetLoaded()
        {
            LoadingTask.Start();

            return LoadingTask;
        }
    }
}
