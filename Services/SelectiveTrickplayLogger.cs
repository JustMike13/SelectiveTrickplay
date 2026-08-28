using System;
using System.IO;
using MediaBrowser.Common.Configuration;

namespace SelectiveTrickplay.Services
{
    /// <summary>
    /// Writes Selective Trickplay messages to a dedicated file in Jellyfin's log directory.
    /// </summary>
    public class SelectiveTrickplayLogger
    {
        private readonly string _logFilePath;
        private readonly object _writeLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectiveTrickplayLogger"/> class.
        /// </summary>
        /// <param name="applicationPaths">Jellyfin application paths.</param>
        public SelectiveTrickplayLogger(IApplicationPaths applicationPaths)
        {
            if (applicationPaths == null)
            {
                throw new ArgumentNullException(nameof(applicationPaths));
            }

            _logFilePath = Path.Combine(applicationPaths.LogDirectoryPath, "SelectiveTrickplay.log");
        }

        /// <summary>
        /// Writes an informational message.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void LogInformation(string message)
        {
            Write("Information", message, null);
        }

        /// <summary>
        /// Writes a warning message.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void LogWarning(string message)
        {
            Write("Warning", message, null);
        }

        /// <summary>
        /// Writes an error message.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="exception">The exception associated with the error.</param>
        public void LogError(string message, Exception? exception = null)
        {
            Write("Error", message, exception);
        }

        private void Write(string level, string message, Exception? exception)
        {
            var entry = string.Format(
                "{0:O} [{1}] {2}{3}",
                DateTimeOffset.UtcNow,
                level,
                message,
                exception == null ? string.Empty : Environment.NewLine + exception);

            lock (_writeLock)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath)!);
                    File.AppendAllText(_logFilePath, entry + Environment.NewLine);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}