using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Utils;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a base class for fetchers.
    /// </summary>
    /// <typeparam name="T">Type of fetched data.</typeparam>
    public abstract class ApiFetcher<T> : IApiFetcher<T>
        where T : class
    {
        /// <inheritdoc/>
        public T? FetchedData { get; private set; }

        /// <summary>
        /// Type of data handled.
        /// </summary>
        protected abstract DataType DataType { get; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Fetching task.
        /// Used to avoid launching multiple fetch operations at the same time.
        /// </summary>
        private Task FetchingTask = Task.CompletedTask;

        /// <summary>
        /// Lock used to start only one fetching task at a time.
        /// </summary>
        private readonly object FetchingTaskLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiFetcher{T}"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        protected ApiFetcher(ILogger logger)
        {
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Result> Fetch()
        {
            Task fetchingTask;
            bool isWaitingForPreviousFetching;

            lock (FetchingTaskLock)
            {
                isWaitingForPreviousFetching = !FetchingTask.IsCompleted;

                if (!isWaitingForPreviousFetching && FetchedData == null)
                {
                    FetchingTask = ExecuteFetchAndStoreData();
                }

                fetchingTask = FetchingTask;
            }

            if (isWaitingForPreviousFetching)
            {
                Logger.LogInformation(Properties.Resources.WaitingForPreviousFetching, DataType.ToString());
                await fetchingTask;
                Logger.LogInformation(Properties.Resources.PreviousFetchingFinished, DataType.ToString());
            }
            else
            {
                await fetchingTask;
            }

            if (FetchedData == null)
            {
                return Result.Fail(string.Format(Properties.Resources.NoDataFetched, DataType.ToString()));
            }

            return Result.Ok();
        }

        /// <summary>
        /// Builds the path of an API file from its configured template.
        /// </summary>
        /// <param name="pathTemplate">Path template, where <c>{0}</c> is replaced by the game mode API value and <c>{1}</c> by the language.</param>
        /// <param name="gameModeApiValue">Game mode API value.</param>
        /// <param name="language">Language, for translations files.</param>
        /// <returns>Path of the API file.</returns>
        protected static string BuildApiPath(string pathTemplate, string gameModeApiValue, string? language = null)
        {
            string path = pathTemplate.Replace("{0}", gameModeApiValue);

            if (language != null)
            {
                path = path.Replace("{1}", language);
            }

            return path;
        }

        /// <summary>
        /// Executes the fetch operation.
        /// </summary>
        /// <returns>Fetched data.</returns>
        protected abstract Task<Result<T>> ExecuteFetch();

        /// <summary>
        /// Executes the fetch operation and stores the fetched data when it succeeds.
        /// </summary>
        private async Task ExecuteFetchAndStoreData()
        {
            Result<T> fetchResult = await ExecuteFetch();

            if (fetchResult.IsSuccess)
            {
                FetchedData = fetchResult.Value;
            }
        }
    }
}
