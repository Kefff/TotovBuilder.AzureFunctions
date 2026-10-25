namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionnalities of a traders fetcher.
    /// The fetched data are the merchant names by trader ID.
    /// </summary>
    public interface ITradersFetcher : IApiFetcher<IReadOnlyDictionary<string, string>>
    {
        /// <summary>
        /// Gets the name of the merchant corresponding to a fetched trader.
        /// </summary>
        /// <param name="traderId">Trader ID.</param>
        /// <returns>Merchant name.</returns>
        /// <exception cref="InvalidDataException">Thrown when the trader has not been fetched.</exception>
        string GetMerchantName(string traderId);
    }
}
