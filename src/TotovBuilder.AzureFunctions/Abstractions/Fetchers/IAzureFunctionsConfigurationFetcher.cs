using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of an Azure Functions configuration fetcher.
    /// </summary>
    public interface IAzureFunctionsConfigurationFetcher : IApiFetcher<AzureFunctionsConfiguration>
    {
    }
}
