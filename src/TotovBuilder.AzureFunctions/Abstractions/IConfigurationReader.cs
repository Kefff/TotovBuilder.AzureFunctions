using FluentResults;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a configuration reader.
    /// </summary>
    public interface IConfigurationReader
    {
        /// <summary>
        /// Reads an integer value from the configuration.
        /// </summary>
        /// <param name="key">Key to read.</param>
        /// <returns>Value if the key is found; otherwise null.</returns>
        int ReadInt(string key);

        /// <summary>
        /// Reads a string value from the configuration.
        /// </summary>
        /// <param name="key">Key to read.</param>
        /// <returns>Value if the key is found; otherwise null.</returns>
        string ReadString(string key);
    }
}
