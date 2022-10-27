using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;

[assembly: FunctionsStartup(typeof(TotovBuilder.AzureFunctions.Startup))]

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents the function startup configuration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        /// <inheritdoc/>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // https://www.codeproject.com/Articles/5276054/Adding-Application-Insights-to-Azure-Functions 
            // https://stackoverflow.com/a/55699619
            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddHttpClient();
            
            builder.Services.AddSingleton<IArmorPenetrationsFetcher, ArmorPenetrationsFetcher>();
            builder.Services.AddSingleton<IAzureFunctionsConfigurationReader, AzureFunctionsConfigurationReader>();
            builder.Services.AddSingleton<IAzureFunctionsConfigurationFetcher, AzureFunctionsConfigurationFetcher>();
            builder.Services.AddSingleton<IAzureFunctionsConfigurationWrapper, AzureFunctionsConfigurationWrapper>();
            builder.Services.AddSingleton<IBartersFetcher, BartersFetcher>();
            builder.Services.AddSingleton<IBlobFetcher, BlobFetcher>();
            builder.Services.AddSingleton<ICache, Cache>();
            builder.Services.AddSingleton<IChangelogFetcher, ChangelogFetcher>();
            builder.Services.AddSingleton<IHttpClientWrapperFactory, HttpClientWrapperFactory>();
            builder.Services.AddSingleton<IItemCategoriesFetcher, ItemCategoriesFetcher>();
            builder.Services.AddSingleton<IItemMissingPropertiesFetcher, ItemMissingPropertiesFetcher>();
            builder.Services.AddSingleton<IItemsFetcher, ItemsFetcher>();
            builder.Services.AddSingleton<IPresetsFetcher, PresetsFetcher>();
            builder.Services.AddSingleton<IPricesFetcher, PricesFetcher>();
            builder.Services.AddSingleton<ITarkovValuesFetcher, TarkovValuesFetcher>();
            builder.Services.AddSingleton<IWebsiteConfigurationFetcher, WebsiteConfigurationFetcher>();
        }
    }
}
