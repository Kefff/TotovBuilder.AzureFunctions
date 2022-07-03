using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
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
            
            builder.Services.AddSingleton<IBartersFetcher, BartersFetcher>();
            builder.Services.AddSingleton<IBlobFetcher, BlobFetcher>();
            builder.Services.AddSingleton<ICache, Cache>();
            builder.Services.AddSingleton<IConfigurationReader, ConfigurationReader>();
            builder.Services.AddSingleton<IHttpClientWrapperFactory, HttpClientWrapperFactory>();
            builder.Services.AddSingleton<IItemCategoriesFetcher, ItemCategoriesFetcher>();
            builder.Services.AddSingleton<IItemsFetcher, ItemsFetcher>();
            builder.Services.AddSingleton<IItemsMetadataFetcher, ItemsMetadataFetcher>();
            builder.Services.AddSingleton<IPresetsFetcher, PresetsFetcher>();
            builder.Services.AddSingleton<IPricesFetcher, PricesFetcher>();
            builder.Services.AddSingleton<IQuestsFetcher, QuestsFetcher>();
            
        }
    }
}
