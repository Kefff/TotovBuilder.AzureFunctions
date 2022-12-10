using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TotovBuilder.AzureFunctions;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((HostBuilderContext hopstBuilderContext, IServiceCollection serviceCollection) =>
    {
        serviceCollection.AddApplicationInsightsTelemetryWorkerService();

        serviceCollection.AddHttpClient();

        serviceCollection.AddSingleton<IArmorPenetrationsFetcher, ArmorPenetrationsFetcher>();
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
    })
    .Build();

host.Run();
