using System;

namespace TotovBuilder.AzureFunctions.Test
{
    /// <summary>
    /// Represents a mock for a logger scope.
    /// </summary>
    public class LoggerScopeMock : IDisposable
    {
        /// <summary>
        /// Instance.
        /// </summary>
        public static LoggerScopeMock Instance { get; } = new LoggerScopeMock();

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerScopeMock"/> class.
        /// </summary>
        private LoggerScopeMock() { }

        /// </<inheritdoc/>
        public void Dispose() { }
    }
}
