using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a base class for API fetchers.
    /// </summary>
    public abstract class ApiFetcher<T> : IApiFetcher<T>
        where T: class
    {
        /// <summary>
        /// API query key.
        /// </summary>
        protected abstract string ApiQueryKey { get; }

        /// <summary>
        /// Configuration reader;
        /// </summary>
        protected readonly IConfigurationReader ConfigurationReader;

        /// <summary>
        /// Type of data handled.
        /// </summary>
        protected abstract DataType DataType { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// API query.
        /// </summary>
        private readonly string ApiQuery;

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
        /// <param name="configurationReader">Configuration reader.</param>
        protected ApiFetcher(ILogger logger, IHttpClientWrapperFactory httpClientWrapperFactory, IConfigurationReader configurationReader, ICache cache)
        {
            Cache = cache;
            ConfigurationReader = configurationReader;
            HttpClientWrapperFactory = httpClientWrapperFactory;
            Logger = logger;
            
            ApiQuery = ConfigurationReader.ReadString(ApiQueryKey);
            ApiUrl = ConfigurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.ApiUrlKey);
            FetchTimeout = ConfigurationReader.ReadInt(TotovBuilder.AzureFunctions.ConfigurationReader.FetchTimeoutKey);
        }

        /// <summary>
        /// Fetches data.
        /// </summary>
        /// <returns>Data fetched as a JSON string.</returns>
        public async Task<T?> Fetch()
        {
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
        /// Gets data from a the content of a fetch response.
        /// </summary>
        /// <param name="responseContent">Content of a fetch response.</param>
        /// <returns>Data.</returns>
        protected abstract Result<T> GetData(string responseContent);

        /// <summary>
        /// Executes the fetch operation.
        /// </summary>
        /// <returns>Fetched data as a JSON string.</returns>
        private async Task<Result<T>> ExecuteFetch()
        {
            if (string.IsNullOrWhiteSpace(ApiUrl)
                || string.IsNullOrWhiteSpace(ApiQuery))
            {
                Logger.LogError(string.Format(Properties.Resources.InvalidConfiguration));

                return Result.Fail(string.Empty);
            }

            Logger.LogInformation(string.Format(Properties.Resources.StartFetching, DataType.ToString()));

            string responseContent;
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string content = JsonSerializer.Serialize(new { Query = ApiQuery }, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            try
            {
                IHttpClientWrapper client = HttpClientWrapperFactory.Create();
                Task<HttpResponseMessage> fetchTask = client.SendAsync(request);

                if (!fetchTask.Wait(FetchTimeout * 1000))
                {
                    Logger.LogError(string.Format(Properties.Resources.FetchingDelayExceeded, DataType.ToString()));

                    return Result.Fail(string.Empty);
                }

                responseContent = await fetchTask.Result.Content.ReadAsStringAsync();

                Logger.LogInformation(string.Format(Properties.Resources.FetchingResponse, DataType.ToString(), fetchTask.Result.ToString()));
                Logger.LogInformation(string.Format(Properties.Resources.FetchingResponseContent, DataType.ToString(), responseContent));
            }
            catch (Exception e)
            {
                Logger.LogError(Properties.Resources.FetchingError, DataType.ToString(), e);

                return Result.Fail(string.Empty);
            }
            
            Result<T> result = GetData(responseContent);

            Logger.LogInformation(string.Format(Properties.Resources.EndFetching, DataType.ToString()));

            return result;
        }
    }
}
