using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a website configuration fetcher.
    /// </summary>
    public interface IWebsiteConfigurationFetcher : IApiFetcher<WebsiteConfiguration>
    {
    }
}
