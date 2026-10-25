using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Utils;
using static TotovBuilder.AzureFunctions.Utils.JsonDeserializer;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a quests fetcher.
    /// </summary>
    public class QuestsFetcher : ApiFetcher<IReadOnlyDictionary<string, Quest>>, IQuestsFetcher
    {
        /// <summary>
        ///  Game mode for which quests are fetched from the API.
        /// </summary>
        public GameMode GameMode { get; init; }

        /// <summary>
        ///  Language in which quests are fetched from the API.
        /// </summary>
        public string Language { get; init; }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Quests;
            }
        }

        /// <summary>
        /// API requester.
        /// </summary>
        private readonly IApiRequester ApiRequester;

        /// <summary>
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestsFetcher"/> class.
        /// </summary>
        /// <param name="gameMode">Game mode for which quests are fetched.</param>
        /// <param name="language">Language in which quests are fetched.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="apiRequester">API requester.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public QuestsFetcher(
            GameMode gameMode,
            string language,
            ILogger<QuestsFetcher> logger,
            IApiRequester apiRequester,
            IConfigurationWrapper configurationWrapper)
            : base(logger)
        {
            ApiRequester = apiRequester;
            ConfigurationWrapper = configurationWrapper;
            GameMode = gameMode;
            Language = language;
        }

        /// <inheritdoc/>
        public Quest GetQuest(string questId)
        {
            if (FetchedData == null || !FetchedData.TryGetValue(questId, out Quest? quest))
            {
                throw new InvalidDataException(string.Format(Properties.Resources.QuestNotFound, questId));
            }

            return quest;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IReadOnlyDictionary<string, Quest>>> ExecuteFetch()
        {
            string tasksPath = BuildApiPath(ConfigurationWrapper.Values.ApiTasksPath, GameMode.ApiQueryValue);
            Task<Result<JsonElement>> tasksDataTask = ApiRequester.RequestData(tasksPath);
            Task<Result<Translations>> translationsTask = ApiRequester.RequestTranslations(BuildApiPath(ConfigurationWrapper.Values.ApiTasksTranslationsPath, GameMode.ApiQueryValue, Language));

            await Task.WhenAll(tasksDataTask, translationsTask);

            Result allTasksResult = Result.Merge(tasksDataTask.Result.ToResult(), translationsTask.Result.ToResult());

            if (allTasksResult.IsFailed)
            {
                return allTasksResult;
            }

            JsonElement tasksDataJson = tasksDataTask.Result.Value;

            if (!TryDeserializeObject(tasksDataJson, "tasks", out JsonElement tasksJson))
            {
                string error = string.Format(Properties.Resources.InvalidApiResponseData, tasksPath, tasksDataJson.GetRawText());
                Logger.LogError(error);

                return Result.Fail(error);
            }

            Dictionary<string, Quest> quests = [];

            foreach (JsonProperty taskJson in tasksJson.EnumerateObject())
            {
                TryDeserializeString(taskJson.Value, "name", out string name);
                TryDeserializeString(taskJson.Value, "wikiLink", out string wikiLink);

                quests[taskJson.Name] = new Quest()
                {
                    Id = taskJson.Name,
                    Name = translationsTask.Result.Value.Translate(name),
                    WikiLink = wikiLink
                };
            }

            return Result.Ok<IReadOnlyDictionary<string, Quest>>(quests);
        }
    }
}
