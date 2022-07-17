namespace TotovBuilder.AzureFunctions.Abstraction
{
    /// <summary>
    /// Provides the functionnalities of an HTTP client wrapper factory.
    /// </summary>
    public interface IHttpClientWrapperFactory
    {
        /// <summary>
        /// Creates an instance of an HTTP client wrapper.
        /// </summary>
        /// <returns>Instance of an HTTP client wrapper.</returns>
        IHttpClientWrapper Create();
    }
}
