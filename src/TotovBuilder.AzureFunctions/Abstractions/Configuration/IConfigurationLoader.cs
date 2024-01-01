namespace TotovBuilder.AzureFunctions.Abstractions.Configuration
{
    /// <summary>
    /// Provides the functionalities of a loader for the configuration of the application.
    /// </summary>
    public interface IConfigurationLoader
    {
        /// <summary>
        /// Loads the configuration of the application.
        /// </summary>
        Task Load();
    }
}
