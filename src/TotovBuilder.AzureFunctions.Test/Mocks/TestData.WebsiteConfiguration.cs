namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static partial class TestData
    {
        public static WebsiteConfiguration WebsiteConfiguration = new WebsiteConfiguration()
        {
            BugReportUrl = "https://discord.gg/bugreport",
            BuildsSortFieldStorageKey = "build_",
            BuildsSortOrderStorageKey = "builds_sort_field",
            BuildStorageKeyPrefix = "builds_sort_order",
            CacheDuration = 3600,
            ChangelogApi = "changelog",
            ContactAddress = "contact@address.com",
            DiscordUrl = "https://discord.gg/server",
            ExportFileExtension = ".ttb",
            ExportFileNamePrefix = "TotovBuilder",
            ExportWarningShowedStoregeKey = "export_warning",
            FetchTimeout = 10,
            GithubUrl = "https://github.com/User/Reposiory",
            ItemCategoriesApi = "itemcategories",
            ItemsApiUrl = "items",
            LanguageStorageKey = "language",
            MerchantFilterStorageKey = "merchant_filter",
            NotificationErrorDuration = 10,
            NotificationInformationDuration = 5,
            NotificationSuccessDuration = 5,
            NotificationWarningDuration = 10,
            PresetsApi = "presets",
            PricesApi = "prices",
            QuestsApi = "quests",
            VersionStorageKey = "version"
        };

        public const string WebsiteConfigurationJson = @"{
  ""bugReportUrl"": ""https://discord.gg/bugreport"",
  ""buildsSortFieldStorageKey"": ""build_"",
  ""buildsSortOrderStorageKey"": ""builds_sort_field"",
  ""buildStorageKeyPrefix"": ""builds_sort_order"",
  ""cacheDuration"": 3600,
  ""changelogApi"": ""changelog"",
  ""contactAddress"": ""contact@address.com"",
  ""discordUrl"": ""https://discord.gg/server"",
  ""exportFileExtension"": "".ttb"",
  ""exportFileNamePrefix"": ""TotovBuilder"",
  ""exportWarningShowedStoregeKey"": ""export_warning"",
  ""fetchTimeout"": 10,
  ""githubUrl"": ""https://github.com/User/Reposiory"",
  ""itemCategoriesApi"": ""itemcategories"",
  ""itemsApiUrl"": ""items"",
  ""languageStorageKey"": ""language"",
  ""merchantFilterStorageKey"": ""merchant_filter"",
  ""notificationErrorDuration"": 10,
  ""notificationInformationDuration"": 5,
  ""notificationSuccessDuration"": 5,
  ""notificationWarningDuration"": 10,
  ""presetsApi"": ""presets"",
  ""pricesApi"": ""prices"",
  ""questsApi"": ""quests"",
  ""versionStorageKey"": ""version""
}";
    }
}
