﻿using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.AzureFunctions.Abstractions.Utils;
using TotovBuilder.AzureFunctions.Configuration;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Net;
using TotovBuilder.AzureFunctions.Utils;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((HostBuilderContext hopstBuilderContext, IServiceCollection serviceCollection) =>
    {
        serviceCollection.AddHttpClient();

        serviceCollection.AddApplicationInsightsTelemetryWorkerService();
        serviceCollection.ConfigureFunctionsApplicationInsights();

        serviceCollection.AddSingleton<IConfigurationLoader, ConfigurationLoader>();
        serviceCollection.AddSingleton<IConfigurationWrapper, ConfigurationWrapper>();
        serviceCollection.AddSingleton<IHttpClientWrapperFactory, HttpClientWrapperFactory>();

        serviceCollection.AddSingleton<IArmorPenetrationsFetcher, ArmorPenetrationsFetcher>();
        serviceCollection.AddSingleton<IAzureFunctionsConfigurationFetcher, AzureFunctionsConfigurationFetcher>();
        serviceCollection.AddSingleton<IBartersFetcher, BartersFetcher>();
        serviceCollection.AddSingleton<IAzureBlobManager, AzureBlobManager>();
        serviceCollection.AddSingleton<IChangelogFetcher, ChangelogFetcher>();
        serviceCollection.AddSingleton<IItemCategoriesFetcher, ItemCategoriesFetcher>();
        serviceCollection.AddSingleton<IItemMissingPropertiesFetcher, ItemMissingPropertiesFetcher>();
        serviceCollection.AddSingleton<IItemsFetcher, ItemsFetcher>();
        serviceCollection.AddSingleton<IPresetsFetcher, PresetsFetcher>();
        serviceCollection.AddSingleton<IPricesFetcher, PricesFetcher>();
        serviceCollection.AddSingleton<ITarkovValuesFetcher, TarkovValuesFetcher>();
        serviceCollection.AddSingleton<IWebsiteConfigurationFetcher, WebsiteConfigurationFetcher>();

        //serviceCollection.AddSingleton<IWebsiteDataGenerator, WebsiteDataGenerator>();
        //serviceCollection.AddSingleton<IWebsiteDataUploader, WebsiteDataUploader>();

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
