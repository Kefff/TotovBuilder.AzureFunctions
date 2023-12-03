using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.AzureFunctions.Utils;
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
        /// Configuration wrapper;
        /// </summary>
        protected readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Type of data handled.
        /// </summary>
        protected abstract DataType DataType { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger<ApiFetcher<T>> Logger;

        /// <summary>
        /// Fetched data.
        /// Once data has been fetched and stored in this property, it is never fetched again.
        /// </summary>
        private T? FetchedData { get; set; } = null;

        /// <summary>
        /// Fetching task.
        /// Used to avoid launching multiple fetch operations at the same time.
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
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        protected ApiFetcher(
            ILogger<ApiFetcher<T>> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationWrapper configurationWrapper)
        {
            ConfigurationWrapper = configurationWrapper;
            HttpClientWrapperFactory = httpClientWrapperFactory;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Result<T>> Fetch()
        {
            if (!FetchingTask.IsCompleted)
            {
                Logger.LogInformation(Properties.Resources.StartWaitingForPreviousFetching, DataType.ToString());
                await FetchingTask;
                Logger.LogInformation(Properties.Resources.EndWaitingForPreviousFetching, DataType.ToString());
            }
            else
            {
                FetchingTask = Task.Run(async () =>
                {
                    if (FetchedData != null)
                    {
                        return;
                    }

                    Result<T> fetchResult = await ExecuteFetch();

                    if (fetchResult.IsSuccess)
                    {
                        FetchedData = fetchResult.Value;
                    }
                });
                await FetchingTask;
            }

            if (FetchedData == null)
            {
                return Result.Fail(string.Format(Properties.Resources.NoDataFetched, DataType.ToString()));
            }

            return Result.Ok(FetchedData);
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
                value = propertyJson.GetString()!;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes the fetch operation.
        /// </summary>
        /// <returns>Fetched data.</returns>
        private async Task<Result<T>> ExecuteFetch()
        {
            if (string.IsNullOrWhiteSpace(ConfigurationWrapper.Values.ApiUrl)
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
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ConfigurationWrapper.Values.ApiUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string content = JsonSerializer.Serialize(new { Query = ApiQuery }, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");

                IHttpClientWrapper client = HttpClientWrapperFactory.Create();
                Task<HttpResponseMessage> fetchTask = client.SendAsync(request);

                if (!fetchTask.Wait(ConfigurationWrapper.Values.FetchTimeout * 1000))
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
