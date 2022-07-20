namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a change in a changelog.
    /// </summary>
    public class ChangelogChange
    {
        /// <summary>
        /// Language.
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Text.
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }
}
