using System;

namespace SophicIoTManager.Models
{
    /// <summary>
    /// Represents the severity level of a log entry.
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success
    }

    /// <summary>
    /// Represents a timestamped log entry for the system audit trail.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// The timestamp when this log entry was created.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The log message content.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The severity level of this log entry.
        /// </summary>
        public LogLevel Level { get; }

        /// <summary>
        /// Gets a formatted timestamp string for display.
        /// </summary>
        public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss");

        /// <summary>
        /// Gets the full formatted log entry for display.
        /// </summary>
        public string FormattedEntry => $"[{FormattedTimestamp}] [{Level}] {Message}";

        /// <summary>
        /// Gets the color indicator based on log level.
        /// </summary>
        public string LevelColor => Level switch
        {
            LogLevel.Info => "#3498DB",      // Blue
            LogLevel.Warning => "#F39C12",   // Orange
            LogLevel.Error => "#E74C3C",     // Red
            LogLevel.Success => "#27AE60",   // Green
            _ => "#95A5A6"                   // Gray
        };

        /// <summary>
        /// Creates a new log entry with the current timestamp.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="level">The severity level (defaults to Info).</param>
        public LogEntry(string message, LogLevel level = LogLevel.Info)
        {
            Timestamp = DateTime.Now;
            Message = message;
            Level = level;
        }

        /// <summary>
        /// Factory method for creating an Info log entry.
        /// </summary>
        public static LogEntry Info(string message) => new(message, LogLevel.Info);

        /// <summary>
        /// Factory method for creating a Warning log entry.
        /// </summary>
        public static LogEntry Warning(string message) => new(message, LogLevel.Warning);

        /// <summary>
        /// Factory method for creating an Error log entry.
        /// </summary>
        public static LogEntry Error(string message) => new(message, LogLevel.Error);

        /// <summary>
        /// Factory method for creating a Success log entry.
        /// </summary>
        public static LogEntry Success(string message) => new(message, LogLevel.Success);
    }
}
