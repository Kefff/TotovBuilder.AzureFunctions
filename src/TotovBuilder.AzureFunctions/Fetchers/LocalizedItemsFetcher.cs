using FluentResults;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Utils;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a fetcher that fetches localized items.
    /// </summary>
    public class LocalizedItemsFetcher : ILocalizedItemsFetcher
    {
        /// <inheritdoc/>
        public IEnumerable<LocalizedItems>? FetchedData
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public Task<Result> Fetch()
        {
            throw new NotImplementedException();
        }
    }
}
