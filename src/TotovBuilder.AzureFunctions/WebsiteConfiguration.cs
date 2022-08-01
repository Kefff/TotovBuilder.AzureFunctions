namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents the Totov Builder website configuration.
    /// </summary>
    public class WebsiteConfiguration
    {
        /// <summary>
        /// Storage key prefix for builds.
        /// </summary>
        public string BuildStorageKeyPrefix { get; set; } = string.Empty;

        /// <summary>
        /// Storage key for the current build sorting colum.
        /// </summary>
        public string BuildsSortFieldStorageKey { get; set; } = string.Empty;

        /// <summary>
        /// Storage key for the current build sort order.
        /// </summary>
        public string BuildsSortOrderStorageKey { get; set; } = string.Empty;

        /// <summary>
        /// Changelog API name.
        /// </summary>
        public string ChangelogApi { get; set; } = string.Empty;

        /// <summary>
        /// Cache dureation (in seconds).
        /// </summary>
        public int CacheDuration { get; set; }

        /// <summary>
        /// Contact mail address.
        /// </summary>
        public string ContactAddress { get; set; } = string.Empty;

        /// <summary>
        /// Discord server URL.
        /// </summary>
        public string DiscordUrl { get; set; } = string.Empty;

        /// <summary>
        /// Export file extension.
        /// </summary>
        public string ExportFileExtension { get; set; } = string.Empty;

        /// <summary>
        /// Export file prefix.
        /// </summary>
        public string ExportFileNamePrefix { get; set; } = string.Empty;

        /// <summary>
        /// Storage key for the value indicating whether the warning about builds not exported has been displayed.
        /// </summary>
        public string ExportWarningShowedStoregeKey { get; set; } = string.Empty;

        /// <summary>
        /// Fetch timeout (in seconds).
        /// </summary>
        public int FetchTimeout { get; set; }

        /// <summary>
        /// Github project URL.
        /// </summary>
        public string GithubUrl { get; set; } = string.Empty;

        /// <summary>
        /// Item categories API name.
        /// </summary>
        public string ItemCategoriesApi { get; set; } = string.Empty;

        /// <summary>
        /// Items API name.
        /// </summary>
        public string ItemsApiUrl { get; set; } = string.Empty;

        /// <summary>
        /// Storage key for the current language.
        /// </summary>
        public string LanguageStorageKey { get; set; } = string.Empty;

        /// <summary>
        /// Storage key for the current merchant filter.
        /// </summary>
        public string MerchantFilterStorageKey { get; set; } = string.Empty;

        /// <summary>
        /// Error notifications duration (in seconds).
        /// </summary>
        public int NotificationErrorDuration { get; set; }

        /// <summary>
        /// Information notifications duration (in seconds).
        /// </summary>
        public int NotificationInformationDuration { get; set; }
        
        /// <summary>
        /// Success notifications duration (in seconds).
        /// </summary>
        public int NotificationSuccessDuration { get; set; }
        
        /// <summary>
        /// Warning notifications duration (in seconds).
        /// </summary>
        public int NotificationWarningDuration { get; set; }

        /// <summary>
        /// Presets API name.
        /// </summary>
        public string PresetsApi { get; set; } = string.Empty;

        /// <summary>
        /// Prices API name.
        /// </summary>
        public string PricesApi { get; set; } = string.Empty;

        /// <summary>
        /// Quests API name.
        /// </summary>
        public string QuestsApi { get; set; } = string.Empty;

        /// <summary>
        /// Bug report URL.
        /// </summary>
        public string BugReportUrl { get; set; } = string.Empty;

        /// <summary>
        /// Storage key for the current version.
        /// </summary>
        public string VersionStorageKey { get; set; } = string.Empty;
    }
}
