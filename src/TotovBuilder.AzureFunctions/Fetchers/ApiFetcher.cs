using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a base class for API fetchers.
    /// </summary>
    public abstract class ApiFetcher<T> : IApiFetcher<T>
        where T: class
    {
        /// <summary>
        /// API query.
        /// </summary>
        protected abstract string ApiQuery { get; }

        /// <summary>
        /// Configuration reader;
        /// </summary>
        protected readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

        /// <summary>
        /// Type of data handled.
        /// </summary>
        protected abstract DataType DataType { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// API URL.
        /// </summary>
        private readonly string ApiUrl;

        /// <summary>
        /// Cache.
        /// </summary>
        private readonly ICache Cache;

        /// <summary>
        /// Fake task used to avoid launching multiple fetch operations at the same time.
        /// </summary>
        private Task FetchingTask = Task.CompletedTask;

        /// <summary>
        /// Fetch timeout in seconds.
        /// </summary>
        private readonly int FetchTimeout;

        /// <summary>
        /// HTTP client wrapper factory.
        /// </summary>
        private readonly IHttpClientWrapperFactory HttpClientWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        protected ApiFetcher(ILogger logger, IHttpClientWrapperFactory httpClientWrapperFactory, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, ICache cache)
        {
            Cache = cache;
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader ;
            HttpClientWrapperFactory = httpClientWrapperFactory;
            Logger = logger;
            
            ApiUrl = AzureFunctionsConfigurationReader.Values.ApiUrl;
            FetchTimeout = AzureFunctionsConfigurationReader.Values.FetchTimeout;
        }

        /// <summary>
        /// Fetches data.
        /// </summary>
        /// <returns>Data fetched as a JSON string.</returns>
        public async Task<T?> Fetch()
        {
            await AzureFunctionsConfigurationReader.WaitUntilReady(); // Awaiting for the configuration to be loaded

            if (!FetchingTask.IsCompleted)
            {
                Logger.LogInformation(string.Format(Properties.Resources.StartWaitingForPreviousFetching, DataType.ToString()));

                await FetchingTask;

                Logger.LogInformation(string.Format(Properties.Resources.EndWaitingForPreviousFetching, DataType.ToString()));

                return Cache.Get<T>(DataType);
            }

            if (Cache.HasValidCache(DataType))
            {
                T? cachedValue = Cache.Get<T>(DataType);
                Logger.LogInformation(string.Format(Properties.Resources.FetchedFromCache, DataType.ToString()));

                return cachedValue;
            }
            
            T? result;
            FetchingTask = new Task(() => { });
            Result<T> fetchResult = await ExecuteFetch();

            if (fetchResult.IsSuccess)
            {
                result = fetchResult.Value;
                Cache.Store(DataType, result);
            }
            else
            {
                result = Cache.Get<T>(DataType);
            }
                  
            FetchingTask.Start();
            await FetchingTask;

            return result;
        }

        /// <summary>
        /// Deserializes data from a the content of a fetch response.
        /// </summary>
        /// <param name="responseContent">Content of a fetch response.</param>
        /// <returns>Deserialized data.</returns>
        protected abstract Task<Result<T>> DeserializeData(string responseContent);

        /// <summary>
        /// Executes the fetch operation.
        /// </summary>
        /// <returns>Fetched data as a JSON string.</returns>
        private async Task<Result<T>> ExecuteFetch()
        {
            if (string.IsNullOrWhiteSpace(ApiUrl)
                || string.IsNullOrWhiteSpace(ApiQuery))
            {
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
            }

            Logger.LogInformation(string.Format(Properties.Resources.StartFetching, DataType.ToString()));

            string responseContent;

            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string content = JsonSerializer.Serialize(new { Query = ApiQuery }, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");

                IHttpClientWrapper client = HttpClientWrapperFactory.Create();
                Task<HttpResponseMessage> fetchTask = client.SendAsync(request);

                if (!fetchTask.Wait(FetchTimeout * 1000))
                {
                    string error = string.Format(Properties.Resources.FetchingDelayExceeded, DataType.ToString());
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                responseContent = await fetchTask.Result.Content.ReadAsStringAsync();

                Logger.LogInformation(string.Format(Properties.Resources.FetchingResponse, DataType.ToString(), fetchTask.Result.ToString()));
                Logger.LogInformation(string.Format(Properties.Resources.FetchingResponseContent, DataType.ToString(), responseContent));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.FetchingError, DataType.ToString(), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }

            Result<string> isolatedDataResult = GetData(responseContent);

            if (!isolatedDataResult.IsSuccess)
            {
                return isolatedDataResult.ToResult<T>();
            }

            Result<T> deserializedDataResult = await DeserializeData(isolatedDataResult.Value);

            Logger.LogInformation(string.Format(Properties.Resources.EndFetching, DataType.ToString()));

            return deserializedDataResult;
        }

        /// <summary>
        /// Checks the validity of an API response data and gets its useful content.
        /// </summary>
        /// <param name="responseContent">API response content.</param>
        /// <returns>Useful content of the API response content.</returns>
        private Result<string> GetData(string responseContent)
        {
            try
            {
                JsonElement response = JsonDocument.Parse(responseContent).RootElement;

                if (!response.TryGetProperty("data", out JsonElement data))
                {
                    string error = string.Format(Properties.Resources.InvalidApiResponseData, DataType.ToString(), responseContent);
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                Match usefulDataKeyMatch = Regex.Match(ApiQuery, "^{ ?([a-zA-Z]+)");

                if (!data.TryGetProperty(usefulDataKeyMatch.Groups[1].Value, out JsonElement isolatedData))
                {
                    string error = string.Format(Properties.Resources.InvalidApiResponseData, DataType.ToString(), responseContent);
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                string isolatedDataRawText = isolatedData.GetRawText();

                if (string.IsNullOrWhiteSpace(isolatedDataRawText) || isolatedDataRawText == "\"\"" || isolatedDataRawText == "[]" || isolatedDataRawText == "{}")
                {
                    string error = string.Format(Properties.Resources.InvalidApiResponseData, DataType.ToString(), responseContent);
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                return Result.Ok(isolatedDataRawText);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.InvalidApiResponseData, DataType.ToString(), string.Join(Environment.NewLine + Environment.NewLine, responseContent, e));
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }
    }
}
