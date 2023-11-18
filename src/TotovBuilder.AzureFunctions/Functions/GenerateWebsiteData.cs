using Microsoft.Azure.Functions.Worker;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Generators;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that generates data for the website and uploads the generated files to the website Azure Blob storage.
    /// </summary>
    public class GenerateWebsiteData
    {
        /// <summary>
        /// Configuration loader.
        /// </summary>
        private readonly IConfigurationLoader ConfigurationLoader;

        /// <summary>
        /// Website data generator.
        /// </summary>
        private readonly IWebsiteDataGenerator WebsiteDataGenerator;

        /// <summary>
        /// Website data uploader.
        /// </summary>
        private readonly IWebsiteDataUploader WebsiteDataUploader;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateWebsiteData"/> class.
        /// </summary>
        /// <param name="configurationLoader">Configuration loader.</param>
        /// <param name="websiteDataGenerator">Website data generator.</param>
        /// <param name="websiteDataUploader">Website data uploader.</param>
        public GenerateWebsiteData(
            IConfigurationLoader configurationLoader,
            IWebsiteDataGenerator websiteDataGenerator,
            IWebsiteDataUploader websiteDataUploader)
        {
            ConfigurationLoader = configurationLoader;
            WebsiteDataGenerator = websiteDataGenerator;
            WebsiteDataUploader = websiteDataUploader;
        }

        [Function("GenerateWebsiteData")]
        public async Task Run([TimerTrigger("%TOTOVBUILDER_GenerateWebsiteDataSchedule%")] ScheduleTrigger scheduleTrigger)
        {
            await ConfigurationLoader.Load();
            Dictionary<string, string> data = await WebsiteDataGenerator.Generate();
            await WebsiteDataUploader.Upload(data);
        }
    }
}
