using TotovBuilder.AzureFunctions.Abstractions.Generators;

namespace TotovBuilder.AzureFunctions.Generators
{
    /// <summary>
    /// Represents a generator that generates data for the website.
    /// </summary>
    public class WebsiteDataGenerator : IWebsiteDataGenerator
    {
        /// <inheritdoc/>
        public Task<Dictionary<string, string>> Generate()
        {
            throw new NotImplementedException();
        }
    }
}
