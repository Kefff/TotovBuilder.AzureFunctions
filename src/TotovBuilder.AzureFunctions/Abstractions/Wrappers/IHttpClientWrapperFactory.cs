namespace TotovBuilder.AzureFunctions.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionnalities of a <see cref="IHttpClientWrapper"/> factory.
    /// </summary>
    public interface IHttpClientWrapperFactory
    {
        /// <summary>
        /// Creates an instance of an <see cref="IHttpClientWrapper"/>.
        /// </summary>
        /// <returns>Instance.</returns>
        IHttpClientWrapper Create();
    }
}
