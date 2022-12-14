using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        // Application Insights configuration
        // Cf https://github.com/Azure/azure-functions-dotnet-worker/issues/1182#issuecomment-1317230604
        builder
            .AddApplicationInsights()
            .AddApplicationInsightsLogger();
    })
    .ConfigureServices((HostBuilderContext hopstBuilderContext, IServiceCollection serviceCollection) =>
    {
        serviceCollection.AddHttpClient();

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
            LoggerFilterRule? filtersToRemove = options.Rules.FirstOrDefault(rule =>
                rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

            if (filtersToRemove != null)
            {
                options.Rules.Remove(filtersToRemove);
            }
        });
    })
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        // Configuring the application using appsettings.json file
        config.AddJsonFile("appsettings.json", optional: true);
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        // Making sure the configuration of the appsettings.json file is used
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    })
    .Build();

host.Run();
