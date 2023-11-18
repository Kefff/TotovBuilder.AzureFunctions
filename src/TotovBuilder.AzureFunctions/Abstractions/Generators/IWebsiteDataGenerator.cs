namespace TotovBuilder.AzureFunctions.Abstractions.Generators
{
    /// <summary>
    /// Provides the functionalities of a generator that generates data for the website.
    /// </summary>
    public interface IWebsiteDataGenerator
    {
        /// <summary>
        /// Generated data for the website.
        /// </summary>
        /// <returns>Pieces of data associated with the name of the blob in which each must be stored.</returns>
        public Task<Dictionary<string, string>> Generate();
    }
}
