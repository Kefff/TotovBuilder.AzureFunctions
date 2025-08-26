using FluentResults;
using TotovBuilder.Model.Utils;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a fetcher that fetches localized items.
    /// </summary>
    public interface ILocalizedItemsFetcher
    {
        /// <summary>
        /// Fetched data.
        /// Once data has been fetched and stored in this property, it is never fetched again.
        /// </summary>
        IEnumerable<LocalizedItems>? FetchedData { get; }

        /// <summary>
        /// Fetches data from the API.
        /// </summary>
        /// <returns>Data fetch result.</returns>
        Task<Result> Fetch();
    }
}
