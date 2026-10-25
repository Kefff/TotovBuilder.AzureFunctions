using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a requester of API endpoints.
    /// </summary>
    public class ApiRequester : IApiRequester
    {
        /// <summary>
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Data request tasks by endpoint path.
        /// </summary>
        private readonly ConcurrentDictionary<string, Lazy<Task<Result<JsonElement>>>> DataRequestTasks = new();

        /// <summary>
        /// HTTP client wrapper factory.
        /// </summary>
        private readonly IHttpClientWrapperFactory HttpClientWrapperFactory;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<ApiRequester> Logger;

        /// <summary>
        /// Translations request tasks by endpoint path.
        /// </summary>
        private readonly ConcurrentDictionary<string, Lazy<Task<Result<Translations>>>> TranslationsRequestTasks = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRequester"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public ApiRequester(
            ILogger<ApiRequester> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationWrapper configurationWrapper)
        {
            ConfigurationWrapper = configurationWrapper;
            HttpClientWrapperFactory = httpClientWrapperFactory;
            Logger = logger;
        }

        /// <inheritdoc/>
        public Task<Result<JsonElement>> RequestData(string path)
        {
            return GetOrAddRequestTask(DataRequestTasks, path, ExecuteRequest);
        }

        /// <inheritdoc/>
        public Task<Result<Translations>> RequestTranslations(string path)
        {
            return GetOrAddRequestTask(TranslationsRequestTasks, path, async p =>
            {
                Result<JsonElement> dataResult = await RequestData(p);

                if (dataResult.IsFailed)
                {
                    return dataResult.ToResult<Translations>();
                }

                return Result.Ok(new Translations(dataResult.Value));
            });
        }

        /// <summary>
        /// Executes the request of an endpoint.
        /// </summary>
        /// <param name="path">Path of the endpoint.</param>
        /// <returns>Content of the <c>data</c> property of the endpoint response.</returns>
        private async Task<Result<JsonElement>> ExecuteRequest(string path)
        {
            if (string.IsNullOrWhiteSpace(ConfigurationWrapper.Values.ApiBaseUrl))
            {
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
            }

            Logger.LogInformation(Properties.Resources.RequestingApi, path);

            string responseContent;

            try
            {
                responseContent = await GetResponseContent(path).WaitAsync(TimeSpan.FromSeconds(ConfigurationWrapper.Values.ExecutionTimeout));
            }
            catch (TimeoutException)
            {
                string error = string.Format(Properties.Resources.ExecutionDelayExceeded, path);
                Logger.LogError(error);

                return Result.Fail(error);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ApiRequestError, path, e);
                Logger.LogError(error);

                return Result.Fail(error);
            }

            Result<JsonElement> dataResult = GetData(path, responseContent);

            if (dataResult.IsSuccess)
            {
                Logger.LogInformation(Properties.Resources.ApiResponseReceived, path);
            }

            return dataResult;
        }

        /// <summary>
        /// Checks the validity of the response of an endpoint and gets its <c>data</c> property.
        /// </summary>
        /// <param name="path">Path of the endpoint.</param>
        /// <param name="responseContent">Content of the endpoint response.</param>
        /// <returns>Content of the <c>data</c> property.</returns>
        private Result<JsonElement> GetData(string path, string responseContent)
        {
            try
            {
                // The document is not disposed because the returned element is kept in cache and must remain readable
                JsonElement response = JsonDocument.Parse(responseContent).RootElement;

                if (response.ValueKind != JsonValueKind.Object
                    || !response.TryGetProperty("data", out JsonElement data)
                    || IsEmpty(data))
                {
                    string error = string.Format(Properties.Resources.InvalidApiResponseData, path, responseContent);
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                return Result.Ok(data);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.InvalidApiResponseData, path, string.Join(Environment.NewLine + Environment.NewLine, responseContent, e));
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Gets the request task of an endpoint, or starts it when it does not exist.
        /// </summary>
        /// <typeparam name="T">Type of requested data.</typeparam>
        /// <param name="requestTasks">Request tasks by endpoint path.</param>
        /// <param name="path">Path of the endpoint.</param>
        /// <param name="request">Function requesting the endpoint.</param>
        /// <returns>Request result.</returns>
        private static async Task<Result<T>> GetOrAddRequestTask<T>(
            ConcurrentDictionary<string, Lazy<Task<Result<T>>>> requestTasks,
            string path,
            Func<string, Task<Result<T>>> request)
        {
            // Lazy ensures the endpoint is requested only once even when concurrent calls add the same path
            Lazy<Task<Result<T>>> requestTask = requestTasks.GetOrAdd(path, p => new Lazy<Task<Result<T>>>(() => request(p)));
            Result<T> result = await requestTask.Value;

            if (result.IsFailed)
            {
                // Failures are not kept so that the endpoint can be requested again later
                requestTasks.TryRemove(new KeyValuePair<string, Lazy<Task<Result<T>>>>(path, requestTask));
            }

            return result;
        }

        /// <summary>
        /// Gets the content of the response of an endpoint.
        /// </summary>
        /// <param name="path">Path of the endpoint.</param>
        /// <returns>Content of the endpoint response.</returns>
        private async Task<string> GetResponseContent(string path)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, ConfigurationWrapper.Values.ApiBaseUrl.TrimEnd('/') + path);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            IHttpClientWrapper client = HttpClientWrapperFactory.Create();
            using HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Indicates whether a JSON element is empty.
        /// </summary>
        /// <param name="jsonElement">JSON element.</param>
        /// <returns><c>true</c> when the element is null, an empty string, an empty array or an empty object; otherwise <c>false</c>.</returns>
        private static bool IsEmpty(JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Array => jsonElement.GetArrayLength() == 0,
                JsonValueKind.Null or JsonValueKind.Undefined => true,
                JsonValueKind.Object => !jsonElement.EnumerateObject().Any(),
                JsonValueKind.String => string.IsNullOrEmpty(jsonElement.GetString()),
                _ => false
            };
        }
    }
}
