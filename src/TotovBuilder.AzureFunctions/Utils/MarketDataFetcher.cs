using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents a market data fetcher.
    /// </summary>
    public class MarketDataFetcher : IMarketDataFetcher
    {
        /// <summary>
        /// API barter query.
        /// </summary>
        private readonly string? ApiBarterQuery;

        /// <summary>
        /// API price query.
        /// </summary>
        private readonly string? ApiPriceQuery;

        /// <summary>
        /// API URL.
        /// </summary>
        private readonly string? ApiUrl;

        /// <summary>
        /// Fetch timeout in seconds.
        /// </summary>
        private readonly int FetchTimeout;

        /// <summary>
        /// Configuration reader;
        /// </summary>
        private readonly IConfigurationReader ConfigurationReader;

        /// <summary>
        /// HTTP client factory.
        /// </summary>
        private readonly IHttpClientFactory HttpClientFactory;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<MarketDataFetcher> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketDataFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientFactory">HTTP client factory.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        public MarketDataFetcher(ILogger<MarketDataFetcher> logger, IHttpClientFactory httpClientFactory, IConfigurationReader configurationReader)
        {
            ConfigurationReader = configurationReader;
            HttpClientFactory = httpClientFactory;
            Logger = logger;
            
            ApiBarterQuery = ConfigurationReader.ReadString(Utils.ConfigurationReader.ApiBarterQueryKey);
            ApiPriceQuery = ConfigurationReader.ReadString(Utils.ConfigurationReader.ApiPriceQueryKey);
            ApiUrl = ConfigurationReader.ReadString(Utils.ConfigurationReader.ApiUrlKey);
            FetchTimeout = ConfigurationReader.ReadInt(Utils.ConfigurationReader.FetchTimeoutKey);
        }

        /// <inheritdoc/>
        public async Task<Result<string>> Fetch()
        {
            if (string.IsNullOrWhiteSpace(ApiUrl)
                || string.IsNullOrWhiteSpace(ApiPriceQuery))
            {
                return Result.Fail(string.Empty);
            }

            string responseData;

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string content = JsonSerializer.Serialize(new { Query = ApiPriceQuery }, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            try
            {
                HttpClient client = HttpClientFactory.CreateClient();
                Task<HttpResponseMessage> fetchTask = client.SendAsync(request);

                if (!fetchTask.Wait(FetchTimeout * 1000))
                {
                    Logger.LogError(Properties.Resources.FetchingDelayExceeded);

                    return Result.Fail(string.Empty);
                }

                responseData = await fetchTask.Result.Content.ReadAsStringAsync();

                Logger.LogInformation(string.Format(Properties.Resources.MarketDataFetchingResponse, fetchTask.Result.ToString(), responseData));
                Logger.LogInformation(string.Format(Properties.Resources.MarketDataFetchingResponseData, responseData));
            }
            catch (Exception e)
            {
                Logger.LogError(Properties.Resources.MarketDataFetchingError, e);

                return Result.Fail(string.Empty);
            }
            
            Result<string> itemsResult = GetResponseItemsOnly(responseData);

            return itemsResult;
        }

        /// <summary>
        /// Gets an item list from the data of a response.
        /// </summary>
        /// <param name="responseData">Response data.</param>
        /// <returns>Item list.</returns>
        private Result<string> GetResponseItemsOnly(string responseData)
        {
            try
            {
                JsonElement response = JsonDocument.Parse(responseData).RootElement;

                if (!response.TryGetProperty("data", out JsonElement data))
                {
                    Logger.LogError(string.Format(Properties.Resources.InvalidMarketApiResponseData, responseData));

                    return Result.Fail(string.Empty);
                }

                if (!data.TryGetProperty("itemsByType", out JsonElement items))
                {
                    Logger.LogError(string.Format(Properties.Resources.InvalidMarketApiResponseData, responseData));
                
                    return Result.Fail(string.Empty);
                }

                string itemsAsJson = JsonSerializer.Serialize(items);

                if (string.IsNullOrWhiteSpace(itemsAsJson) || itemsAsJson == "\"\"" || itemsAsJson == "[]" || itemsAsJson == "{}")
                {
                    Logger.LogError(string.Format(Properties.Resources.InvalidMarketApiResponseData, responseData));
                
                    return Result.Fail(string.Empty);
                }

                return Result.Ok(itemsAsJson);
            }
            catch (Exception)
            {
                Logger.LogError(string.Format(Properties.Resources.InvalidMarketApiResponseData, responseData));

                return Result.Fail(string.Empty);
            }
        }
    }
}
