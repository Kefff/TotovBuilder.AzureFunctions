using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Utils;

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

            builder.Services.AddSingleton<IMarketDataFetcher, MarketDataFetcher>();
            builder.Services.AddSingleton<IBlobDataFetcher, BlobDataFetcher>();
            builder.Services.AddSingleton<ICache, Cache>();
            builder.Services.AddSingleton<IConfigurationReader, ConfigurationReader>();
            builder.Services.AddSingleton<IDataFetcher, DataFetcher>();
        }
    }
}
