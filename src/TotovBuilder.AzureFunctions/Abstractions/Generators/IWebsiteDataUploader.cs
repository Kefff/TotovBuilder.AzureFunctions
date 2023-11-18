namespace TotovBuilder.AzureFunctions.Abstractions.Generators
{
    /// <summary>
    /// Provides the functionalities of an uploader that uploads data generated for the website.
    /// </summary>
    public interface IWebsiteDataUploader
    {
        /// <summary>
        /// Uploads data generated for the website.
        /// </summary>
        /// <param name="data">Pieces of data associated with the name of the blob in which each must be stored.</param>
        Task Upload(Dictionary<string, string> data);
    }
}
