using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.AzureFunctions.Configuration;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Net;
using TotovBuilder.Shared.Azure;
using TotovBuilder.Shared.Extensions;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((HostBuilderContext hopstBuilderContext, IServiceCollection services) =>
    {
        services.AddHttpClient();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();
        services.AddSingleton<IConfigurationWrapper, ConfigurationWrapper>();
        services.AddSingleton<IHttpClientWrapperFactory, HttpClientWrapperFactory>();

        services.AddSingleton<IArmorPenetrationsFetcher, ArmorPenetrationsFetcher>();
        services.AddSingleton<IAzureFunctionsConfigurationFetcher, AzureFunctionsConfigurationFetcher>();
        services.AddSingleton<IBartersFetcher, BartersFetcher>();
        services.AddSingleton<IChangelogFetcher, ChangelogFetcher>();
        services.AddSingleton<IItemCategoriesFetcher, ItemCategoriesFetcher>();
        services.AddSingleton<IItemMissingPropertiesFetcher, ItemMissingPropertiesFetcher>();
        services.AddSingleton<IItemsFetcher, ItemsFetcher>();
        services.AddSingleton<IPresetsFetcher, PresetsFetcher>();
        services.AddSingleton<IPricesFetcher, PricesFetcher>();
        services.AddSingleton<ITarkovValuesFetcher, TarkovValuesFetcher>();
        services.AddSingleton<IWebsiteConfigurationFetcher, WebsiteConfigurationFetcher>();

        services.AddAzureBlobStorageManager(
            (IServiceProvider serviceProvider) =>
            {
                IConfigurationWrapper configurationWrapper = serviceProvider.GetRequiredService<IConfigurationWrapper>();

                return new AzureBlobStorageManagerOptions(configurationWrapper.Values.AzureBlobStorageConnectionString, configurationWrapper.Values.ExecutionTimeout);
            });

        services.Configure<LoggerFilterOptions>(options =>
        {
            // Removing default filters that only allow warnings and errors to be logged
            // Cf https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#application-insights
            LoggerFilterRule? filtersToRemove = options.Rules.FirstOrDefault(rule =>
                rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

            if (filtersToRemove != null)
            {
                options.Rules.Remove(filtersToRemove);
            }
        });
    })
    .Build();

host.Run();
