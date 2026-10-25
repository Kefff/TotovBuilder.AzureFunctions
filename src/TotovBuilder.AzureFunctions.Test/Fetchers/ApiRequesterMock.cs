using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Test;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents a factory of <see cref="IApiRequester"/> mocks.
    /// </summary>
    public static class ApiRequesterMock
    {
        /// <summary>
        /// Content of a translations file without translations.
        /// </summary>
        public const string EmptyTranslationsJson = @"{
  ""data"": {}
}";

        /// <summary>
        /// Creates a mock returning items, and the same translations for all the languages of the test configuration.
        /// </summary>
        /// <param name="itemsJson">Content of the items file.</param>
        /// <param name="translationsJson">Content of the items translations files.</param>
        /// <returns>Mock.</returns>
        public static Mock<IApiRequester> Create(string itemsJson, string translationsJson = EmptyTranslationsJson)
        {
            return Create(
                new Dictionary<string, string>()
                {
                    { "/regular/items", itemsJson }
                },
                new Dictionary<string, string>()
                {
                    { "/regular/items_en", translationsJson },
                    { "/regular/items_fr", translationsJson }
                });
        }

        /// <summary>
        /// Creates a mock returning files by path.
        /// The fetching of a file that is not provided fails.
        /// </summary>
        /// <param name="dataJsonByPath">Content of data files by path.</param>
        /// <param name="translationsJsonByPath">Content of translations files by path.</param>
        /// <returns>Mock.</returns>
        public static Mock<IApiRequester> Create(Dictionary<string, string> dataJsonByPath, Dictionary<string, string> translationsJsonByPath)
        {
            Mock<IApiRequester> apiRequesterMock = new();
            apiRequesterMock
                .Setup(m => m.RequestData(It.IsAny<string>()))
                .Returns((string path) =>
                {
                    if (!dataJsonByPath.TryGetValue(path, out string? json))
                    {
                        return Task.FromResult(Result.Fail<JsonElement>($"{path} - Not found."));
                    }

                    return Task.FromResult(Result.Ok(GetData(json)));
                });
            apiRequesterMock
                .Setup(m => m.RequestTranslations(It.IsAny<string>()))
                .Returns((string path) =>
                {
                    if (!translationsJsonByPath.TryGetValue(path, out string? json))
                    {
                        return Task.FromResult(Result.Fail<Translations>($"{path} - Not found."));
                    }

                    return Task.FromResult(Result.Ok(new Translations(GetData(json))));
                });

            return apiRequesterMock;
        }

        /// <summary>
        /// Creates a mock returning the files needed to fetch the prices and barters of the "regular" game mode.
        /// </summary>
        /// <param name="itemsJson">Content of the items file, or <c>null</c> when it is not provided.</param>
        /// <param name="bartersJson">Content of the barters file, or <c>null</c> when it is not provided.</param>
        /// <returns>Mock.</returns>
        public static Mock<IApiRequester> CreateForPricesAndBarters(string? itemsJson = null, string? bartersJson = null)
        {
            Dictionary<string, string> dataJsonByPath = new()
            {
                { "/regular/tasks", TestData.TasksJson },
                { "/regular/traders", TestData.TradersJson }
            };

            if (itemsJson != null)
            {
                dataJsonByPath.Add("/regular/items", itemsJson);
            }

            if (bartersJson != null)
            {
                dataJsonByPath.Add("/regular/barters", bartersJson);
            }

            return Create(
                dataJsonByPath,
                new Dictionary<string, string>()
                {
                    { "/regular/tasks_en", TestData.TasksTranslationsJson },
                    { "/regular/tasks_fr", TestData.TasksTranslationsJson }
                });
        }

        /// <summary>
        /// Gets the <c>data</c> property of the content of a file.
        /// </summary>
        /// <param name="json">Content of the file.</param>
        /// <returns><c>data</c> property.</returns>
        private static JsonElement GetData(string json)
        {
            return JsonDocument.Parse(json).RootElement.GetProperty("data");
        }
    }
}
