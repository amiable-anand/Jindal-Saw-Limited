using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Jindal.Services
{
    /// <summary>
    /// Provides comprehensive error handling, logging, and retry mechanisms
    /// </summary>
    public static class ErrorHandlingService
    {
        private static readonly string LogFilePath = Path.Combine(FileSystem.AppDataDirectory, "app_errors.log");
        private static readonly int MaxLogFileSize = 5 * 1024 * 1024; // 5MB
        // Removed unused field - retry attempts are now configurable per method call

        /// <summary>
        /// Logs an error with context information
        /// </summary>
        public static void LogError(string message, Exception? ex = null, string context = "", Dictionary<string, object>? additionalData = null)
        {
            try
            {
                var logEntry = new ErrorLogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Exception = ex?.ToString() ?? string.Empty,
                    Context = context,
                    AdditionalData = additionalData ?? new Dictionary<string, object>()
                };

                // Log to debug console
                System.Diagnostics.Debug.WriteLine($"[ERROR] {logEntry.Timestamp:yyyy-MM-dd HH:mm:ss} - {message}");
                if (ex != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                }

                // Log to file
                WriteToLogFile(logEntry);
            }
            catch (Exception logEx)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log error: {logEx.Message}");
            }
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public static void LogInfo(string message, string context = "")
        {
            try
            {
                var logEntry = new ErrorLogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Context = context,
                    LogLevel = "INFO"
                };

                System.Diagnostics.Debug.WriteLine($"[INFO] {logEntry.Timestamp:yyyy-MM-dd HH:mm:ss} - {message}");
                WriteToLogFile(logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log info: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public static void LogWarning(string message, string context = "")
        {
            try
            {
                var logEntry = new ErrorLogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Context = context,
                    LogLevel = "WARNING"
                };

                System.Diagnostics.Debug.WriteLine($"[WARNING] {logEntry.Timestamp:yyyy-MM-dd HH:mm:ss} - {message}");
                WriteToLogFile(logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes an operation with retry logic
        /// </summary>
        public static async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation, int maxRetries = 3, string operationName = "Operation")
        {
            var lastException = new Exception("Unknown error");
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var result = await operation();
                    if (attempt > 1)
                    {
                        LogInfo($"{operationName} succeeded after {attempt} attempts");
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    if (attempt == maxRetries)
                    {
                        LogError($"{operationName} failed after {maxRetries} attempts", ex, "RetryOperation");
                        throw;
                    }
                    
                    LogWarning($"{operationName} failed on attempt {attempt}, retrying...", "RetryOperation");
                    
                    // Exponential backoff
                    var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt - 1) * 1000);
                    await Task.Delay(delay);
                }
            }

            throw lastException;
        }

        /// <summary>
        /// Executes an operation with retry logic (void return)
        /// </summary>
        public static async Task ExecuteWithRetry(Func<Task> operation, int maxRetries = 3, string operationName = "Operation")
        {
            await ExecuteWithRetry(async () =>
            {
                await operation();
                return true;
            }, maxRetries, operationName);
        }

        /// <summary>
        /// Safely executes an operation and handles exceptions
        /// </summary>
        public static async Task<(bool Success, T? Result, string ErrorMessage)> SafeExecute<T>(Func<Task<T>> operation, string operationName = "Operation")
        {
            try
            {
                var result = await operation();
                return (true, result, string.Empty);
            }
            catch (Exception ex)
            {
                LogError($"{operationName} failed", ex, "SafeExecute");
                return (false, default(T), ex.Message);
            }
        }

        /// <summary>
        /// Safely executes an operation and handles exceptions (void return)
        /// </summary>
        public static async Task<(bool Success, string ErrorMessage)> SafeExecute(Func<Task> operation, string operationName = "Operation")
        {
            try
            {
                await operation();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                LogError($"{operationName} failed", ex, "SafeExecute");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Gets formatted error message for user display
        /// </summary>
        public static string GetUserFriendlyErrorMessage(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException => "Required data is missing. Please check your input.",
                ArgumentException => "Invalid input provided. Please check your data.",
                InvalidOperationException => "Operation cannot be performed at this time. Please try again.",
                UnauthorizedAccessException => "You don't have permission to perform this action.",
                TimeoutException => "The operation timed out. Please check your connection and try again.",
                DirectoryNotFoundException or FileNotFoundException => "Required data could not be found. Please contact support.",
                _ => "An unexpected error occurred. Please try again or contact support if the problem persists."
            };
        }

        /// <summary>
        /// Clears old log entries to prevent excessive disk usage
        /// </summary>
        public static void ClearOldLogs()
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    var fileInfo = new FileInfo(LogFilePath);
                    if (fileInfo.Length > MaxLogFileSize)
                    {
                        // Create backup and clear main log
                        var backupPath = LogFilePath + ".backup";
                        if (File.Exists(backupPath))
                        {
                            File.Delete(backupPath);
                        }
                        File.Move(LogFilePath, backupPath);
                        
                        LogInfo("Log file cleared due to size limit", "LogMaintenance");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear old logs: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets recent log entries for debugging
        /// </summary>
        public static List<ErrorLogEntry> GetRecentLogs(int maxEntries = 50)
        {
            try
            {
                if (!File.Exists(LogFilePath))
                    return new List<ErrorLogEntry>();

                var lines = File.ReadAllLines(LogFilePath);
                var logs = new List<ErrorLogEntry>();

                foreach (var line in lines.TakeLast(maxEntries))
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var logEntry = JsonSerializer.Deserialize<ErrorLogEntry>(line);
                            if (logEntry != null)
                            {
                                logs.Add(logEntry);
                            }
                        }
                    }
                    catch
                    {
                        // Skip invalid log entries
                    }
                }

                return logs.OrderByDescending(l => l.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get recent logs: {ex.Message}");
                return new List<ErrorLogEntry>();
            }
        }

        /// <summary>
        /// Writes log entry to file
        /// </summary>
        private static void WriteToLogFile(ErrorLogEntry logEntry)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(logEntry);
                File.AppendAllText(LogFilePath, jsonString + Environment.NewLine);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates database connection and logs status
        /// </summary>
        public static async Task<bool> ValidateSystemHealth()
        {
            var healthChecks = new List<(string Name, Func<Task<bool>> Check)>
            {
                ("Database Connection", async () => await DatabaseService.TestDatabaseConnection()),
                ("File System Access", () => Task.FromResult(Directory.Exists(FileSystem.AppDataDirectory))),
                ("Preferences Access", () => Task.FromResult(TestPreferencesAccess()))
            };

            var results = new List<string>();
            bool allHealthy = true;

            foreach (var (name, check) in healthChecks)
            {
                try
                {
                    var result = await check();
                    results.Add($"{name}: {(result ? "✓ Healthy" : "✗ Failed")}");
                    if (!result) allHealthy = false;
                }
                catch (Exception ex)
                {
                    results.Add($"{name}: ✗ Error - {ex.Message}");
                    allHealthy = false;
                }
            }

            var healthReport = string.Join("\n", results);
            
            if (allHealthy)
            {
                LogInfo($"System health check passed:\n{healthReport}", "HealthCheck");
            }
            else
            {
                LogWarning($"System health check failed:\n{healthReport}", "HealthCheck");
            }

            return allHealthy;
        }

        /// <summary>
        /// Tests preferences access
        /// </summary>
        private static bool TestPreferencesAccess()
        {
            try
            {
                var testKey = "health_check_test";
                var testValue = DateTime.Now.ToString();
                
                Preferences.Set(testKey, testValue);
                var retrieved = Preferences.Get(testKey, "");
                Preferences.Remove(testKey);
                
                return retrieved == testValue;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Represents a log entry
    /// </summary>
    public class ErrorLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Exception { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string LogLevel { get; set; } = "ERROR";
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
}
