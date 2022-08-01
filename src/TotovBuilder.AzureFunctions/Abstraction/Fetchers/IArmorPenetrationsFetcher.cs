using System.Collections.Generic;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Abstraction.Fetchers
{
    /// <summary>
    /// Provides the functionalities of an armor penetrations fetcher.
    /// </summary>
    public interface IArmorPenetrationsFetcher : IApiFetcher<IEnumerable<ArmorPenetration>>
    {
    }
}
