using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using static System.Text.Json.JsonElement;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a base class for API fetchers.
    /// </summary>
    public abstract class ApiFetcher<T> : IApiFetcher<T>
        where T : class
    {
        /// <summary>
        /// API query.
        /// </summary>
        protected abstract string ApiQuery { get; }

        /// <summary>
        /// Azure Functions configuration wrapper;
        /// </summary>
        protected readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

        /// <summary>
        /// Type of data handled.
        /// </summary>
        protected abstract DataType DataType { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger<ApiFetcher<T>> Logger;

        /// <summary>
        /// Cache.
        /// </summary>
        private readonly ICache Cache;

        /// <summary>
        /// Fake task used to avoid launching multiple fetch operations at the same time.
        /// </summary>
        private Task FetchingTask = Task.CompletedTask;

        /// <summary>
        /// HTTP client wrapper factory.
        /// </summary>
        private readonly IHttpClientWrapperFactory HttpClientWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration wrapper.</param>
        protected ApiFetcher(
            ILogger<ApiFetcher<T>> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, ICache cache)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
            Cache = cache;
            HttpClientWrapperFactory = httpClientWrapperFactory;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<T?> Fetch()
        {
            if (!FetchingTask.IsCompleted)
            {
                Logger.LogInformation(Properties.Resources.StartWaitingForPreviousFetching, DataType.ToString());

                await FetchingTask;

                Logger.LogInformation(Properties.Resources.EndWaitingForPreviousFetching, DataType.ToString());

                return Cache.Get<T>(DataType);
            }

            if (Cache.HasValidCache(DataType))
            {
                T? cachedValue = Cache.Get<T>(DataType);
                Logger.LogInformation(Properties.Resources.FetchedFromCache, DataType.ToString());

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
        /// Tries to deserialize an array.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property to deserialize.</param>
        /// <param name="propertyName">Name of the property to deserialize.</param>
        /// <param name="value">Deserialized value when successful.</param>
        /// <returns><c>true</c> when the deserialization succeeds; otherwise <c>false</c>.</returns>
        protected bool TryDeserializeArray(JsonElement jsonElement, string propertyName, out ArrayEnumerator value)
        {
            value = new ArrayEnumerator();

            if (jsonElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propertyJson))
            {
                return false;
            }

            if (propertyJson.ValueKind == JsonValueKind.Array)
            {
                value = propertyJson.EnumerateArray();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to deserialize a double value.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property to deserialize.</param>
        /// <param name="propertyName">Name of the property to deserialize.</param>
        /// <param name="value">Deserialized value when successful.</param>
        /// <returns><c>true</c> when the deserialization succeeds; otherwise <c>false</c>.</returns>
        protected bool TryDeserializeDouble(JsonElement jsonElement, string propertyName, out double value)
        {
            value = 0;

            if (jsonElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propertyJson))
            {
                return false;
            }

            if (propertyJson.ValueKind == JsonValueKind.Number)
            {
                value = propertyJson.GetDouble();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to deserialize an object value.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property to deserialize.</param>
        /// <param name="propertyName">Name of the property to deserialize.</param>
        /// <param name="value">Deserialized value when successful.</param>
        /// <returns><c>true</c> when the deserialization succeeds; otherwise <c>false</c>.</returns>
        protected bool TryDeserializeObject(JsonElement jsonElement, string propertyName, out JsonElement value)
        {
            value = new JsonElement();

            if (jsonElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (jsonElement.TryGetProperty(propertyName, out JsonElement element))
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    value = element;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to deserialize a string value.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property to deserialize.</param>
        /// <param name="propertyName">Name of the property to deserialize.</param>
        /// <param name="value">Deserialized value when successful.</param>
        /// <returns><c>true</c> when the deserialization succeeds; otherwise <c>false</c>.</returns>
        protected bool TryDeserializeString(JsonElement jsonElement, string propertyName, out string value)
        {
            value = string.Empty;

            if (jsonElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!jsonElement.TryGetProperty(propertyName, out JsonElement propertyJson))
            {
                return false;
            }

            if (propertyJson.ValueKind == JsonValueKind.String)
            {
                value = propertyJson.GetString();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes the fetch operation.
        /// </summary>
        /// <returns>Fetched data as a JSON string.</returns>
        private async Task<Result<T>> ExecuteFetch()
        {
            if (string.IsNullOrWhiteSpace(AzureFunctionsConfigurationReader.Values.ApiUrl)
                || string.IsNullOrWhiteSpace(ApiQuery))
            {
                string error = Properties.Resources.InvalidConfiguration;
                Logger.LogError(error);

                return Result.Fail(error);
            }

            Logger.LogInformation(Properties.Resources.StartFetching, DataType.ToString());

            string responseContent;

            try
            {
                using HttpRequestMessage request = new(HttpMethod.Post, AzureFunctionsConfigurationReader.Values.ApiUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string content = JsonSerializer.Serialize(new { Query = ApiQuery }, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");

                IHttpClientWrapper client = HttpClientWrapperFactory.Create();
                Task<HttpResponseMessage> fetchTask = client.SendAsync(request);

                if (!fetchTask.Wait(AzureFunctionsConfigurationReader.Values.FetchTimeout * 1000))
                {
                    string error = string.Format(Properties.Resources.FetchingDelayExceeded, DataType.ToString());
                    Logger.LogError(error);

                    return Result.Fail(error);
                }

                responseContent = await fetchTask.Result.Content.ReadAsStringAsync();
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

            Logger.LogInformation(Properties.Resources.EndFetching, DataType.ToString());

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
