using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System;
using System.Diagnostics;

namespace AssignmentSystem.Services
{
    /// <summary>
    /// Provides a debug console for students to log messages and for test cases to capture output.
    /// Log entries are now stored persistently in a JSON file at Application.persistentDataPath.
    /// All operations (log, read, clear) use file-based storage with error handling.
    /// </summary>
    public static class AssignmentDebugConsole
    {
        public class LogEntry
        {
            public string Message { get; set; }
            public System.DateTime Timestamp { get; set; }
            public string StackTrace { get; set; }
        }

        private static readonly object _lock = new object();
        private static readonly string LogFilePath = System.IO.Path.Combine(
            UnityEngine.Application.persistentDataPath, "assignment_debug_log.json");

        /// <summary>
        /// Logs a message to the debug console, capturing timestamp and stack trace.
        /// The log entry is appended to a persistent JSON file.
        /// </summary>
        public static void Log(object message)
        {
            log(message, null);
        }

        public static void Log(object message, object context)
        {
            log(message, context);
        }

        private static void log(object message, object context, int stackTraceLevel = 2)
        {
            var message_ = context == null ? message.ToString() : $"{message} {context}";
            lock (_lock)
            {
                var entry = new LogEntry
                {
                    Message = message_,
                    Timestamp = System.DateTime.Now,
                    StackTrace = new System.Diagnostics.StackTrace(stackTraceLevel, true).ToString()
                };
                try
                {
                    string json = JsonConvert.SerializeObject(entry);
                    System.IO.File.AppendAllText(LogFilePath, json + "\n");
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error appending log entry: {ex.Message}");
                }
                // also log message to unity console
                UnityEngine.Debug.Log(message_);
            }
        }

        /// <summary>
        /// Gets the current output of the debug console as a single string.
        /// Reads all log entries from the persistent JSON file.
        /// </summary>
        public static string GetOutput()
        {
            lock (_lock)
            {
                var sb = new StringBuilder();
                var entries = ReadLogEntries();
                foreach (var entry in entries)
                {
                    sb.AppendLine($"{entry.Message}");
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets all log entries (for editor window and advanced features).
        /// Returns a read-only list of log entries from persistent storage.
        /// </summary>
        public static IReadOnlyList<LogEntry> GetEntries()
        {
            lock (_lock)
            {
                var entries = ReadLogEntries();
                return entries.AsReadOnly();
            }
        }

        /// <summary>
        /// Clears the debug console output by overwriting the persistent JSON file with an empty list.
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                try
                {
                    System.IO.File.WriteAllText(LogFilePath, string.Empty);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error clearing log file: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// Helper: Reads log entries from the persistent JSON file.
        /// Returns an empty list if the file does not exist or is invalid.
        /// </summary>
        private static List<LogEntry> ReadLogEntries()
        {
            try
            {
                var entries = new List<LogEntry>();
                if (!System.IO.File.Exists(LogFilePath))
                    return entries;
                foreach (var line in System.IO.File.ReadLines(LogFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    try
                    {
                        var entry = JsonConvert.DeserializeObject<LogEntry>(line);
                        if (entry != null)
                            entries.Add(entry);
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Error parsing log entry: {ex.Message}");
                    }
                }
                return entries;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error reading log file: {ex.Message}");
                return new List<LogEntry>();
            }
        }
    }
}
