using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((HostBuilderContext hopstBuilderContext, IServiceCollection serviceCollection) =>
    {
        serviceCollection.AddHttpClient();

        serviceCollection.AddApplicationInsightsTelemetryWorkerService();
        serviceCollection.ConfigureFunctionsApplicationInsights();

        serviceCollection.AddSingleton<IArmorPenetrationsFetcher, ArmorPenetrationsFetcher>();
        serviceCollection.AddSingleton<IAzureFunctionsConfigurationCache, AzureFunctionsConfigurationCache>();
        serviceCollection.AddSingleton<IAzureFunctionsConfigurationFetcher, AzureFunctionsConfigurationFetcher>();
        serviceCollection.AddSingleton<IAzureFunctionsConfigurationReader, AzureFunctionsConfigurationReader>();
        serviceCollection.AddSingleton<IBartersFetcher, BartersFetcher>();
        serviceCollection.AddSingleton<IBlobFetcher, BlobFetcher>();
        serviceCollection.AddSingleton<ICache, Cache>();
        serviceCollection.AddSingleton<IChangelogFetcher, ChangelogFetcher>();
        serviceCollection.AddSingleton<IHttpClientWrapperFactory, HttpClientWrapperFactory>();
        serviceCollection.AddSingleton<IHttpResponseDataFactory, HttpResponseDataFactory>();
        serviceCollection.AddSingleton<IItemCategoriesFetcher, ItemCategoriesFetcher>();
        serviceCollection.AddSingleton<IItemMissingPropertiesFetcher, ItemMissingPropertiesFetcher>();
        serviceCollection.AddSingleton<IItemsFetcher, ItemsFetcher>();
        serviceCollection.AddSingleton<IPresetsFetcher, PresetsFetcher>();
        serviceCollection.AddSingleton<IPricesFetcher, PricesFetcher>();
        serviceCollection.AddSingleton<ITarkovValuesFetcher, TarkovValuesFetcher>();
        serviceCollection.AddSingleton<IWebsiteConfigurationFetcher, WebsiteConfigurationFetcher>();

        serviceCollection.Configure<LoggerFilterOptions>(options =>
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
