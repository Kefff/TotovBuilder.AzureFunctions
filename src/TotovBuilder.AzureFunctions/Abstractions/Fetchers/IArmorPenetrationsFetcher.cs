using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of an armor penetrations fetcher.
    /// </summary>
    public interface IArmorPenetrationsFetcher : IApiFetcher<IEnumerable<ArmorPenetration>>
    {
    }
}
