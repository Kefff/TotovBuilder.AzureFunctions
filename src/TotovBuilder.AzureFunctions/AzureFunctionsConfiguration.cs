namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents an Azure functions configuration.
    /// </summary>
    public class AzureFunctionsConfiguration
    {
        /// <summary>
        /// API query for getting barters.
        /// </summary>
        public string ApiBartersQuery { get; set; } = string.Empty;

        /// <summary>
        /// API query for getting items.
        /// </summary>
        public string ApiItemsQuery { get; set; } = string.Empty;

        /// <summary>
        /// API query for getting prices.
        /// </summary>
        public string ApiPricesQuery { get; set; } = string.Empty;

        /// <summary>
        /// API query for getting prices.
        /// </summary>
        public string ApiQuestsQuery { get; set; } = string.Empty;

        /// <summary>
        /// API URL.
        /// </summary>
        public string ApiUrl { get; set; } = string.Empty;

        /// <summary>
        /// Name of the Azure blob containing the armor penetrations.
        /// </summary>
        public string AzureArmorPenetrationsBlobName { get; set; } = string.Empty;

        /// <summary>
        /// Connection string to the Azure blob storage containing static data.
        /// </summary>
        public string AzureBlobStorageConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Name of the Azure blob storage containing static data.
        /// </summary>
        public string AzureBlobStorageContainerName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the Azure blob containing the changelog.
        /// </summary>
        public string AzureChangelogBlobName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the Azure blob containing the Azure Functions configuration.
        /// </summary>
        public string AzureFunctionsConfigurationBlobName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the Azure blob containing the item categories.
        /// </summary>
        public string AzureItemCategoriesBlobName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the Azure blob containing the presets.
        /// </summary>
        public string AzurePresetsBlobName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the Azure blob containing the values related to Tarkov gameplay.
        /// </summary>
        public string AzureTarkovValuesBlobName { get; set; } = string.Empty;

        /// <summary>
        /// Time (in seconds) fetched API data is kept before needing to refresh it.
        /// </summary>
        public int CacheDuration { get; set; } = 43200;

        /// <summary>
        /// Time (in seconds) before a fetch is timed out.
        /// </summary>
        public int FetchTimeout { get; set; } = 30;

        /// <summary>
        /// Time (in seconds) fetched API prices are kept before needing to refresh them.
        /// </summary>
        public int PriceCacheDuration { get; set; } = 3600;
    }
}
