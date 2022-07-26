namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents extension methods for the <see cref="string"/> type.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Changes the case of a string to match a string case.
        /// </summary>
        /// <param name="stringToFormat">String to format.</param>
        /// <returns>Formatter string.</returns>
        public static string ToStringCase(this string stringToFormat)
        {
            return stringToFormat[0] + stringToFormat[1..].ToLowerInvariant();
        }
    }
}
