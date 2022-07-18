using System;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a changelog entry.
    /// </summary>
    public class ChangelogEntry
    {
        /// <summary>
        /// Changes.
        /// </summary>
        public ChangelogChange[] Changes = Array.Empty<ChangelogChange>();

        /// <summary>
        /// Date.
        /// </summary>
        public DateTime Date;

        /// <summary>
        /// Version.
        /// </summary>
        public string Version = "1.0.0";
    }
}
