using System.Collections.Generic;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of an armor penetrations fetcher.
    /// </summary>
    public interface IArmorPenetrationsFetcher : IApiFetcher<IEnumerable<ArmorPenetration>>
    {
    }
}
