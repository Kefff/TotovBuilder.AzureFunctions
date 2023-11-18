using TotovBuilder.AzureFunctions.Abstractions.Generators;

namespace TotovBuilder.AzureFunctions.Generators
{
    /// <summary>
    /// Represents an uploader that uploads data generated for the website.
    /// </summary>
    public class WebsiteDataUploader : IWebsiteDataUploader
    {
        public Task Upload(Dictionary<string, string> data)
        {
            throw new NotImplementedException();
        }
    }
}
