using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jindal.Services
{
    /// <summary>
    /// Centralized error handling service for consistent error management across the application
    /// </summary>
    public class ErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService>? _logger;
        private readonly Dictionary<Type, string> _errorMessages;

        public ErrorHandlingService(ILogger<ErrorHandlingService>? logger = null)
        {
            _logger = logger;
            _errorMessages = InitializeErrorMessages();
        }

        private Dictionary<Type, string> InitializeErrorMessages()
        {
            return new Dictionary<Type, string>
            {
                { typeof(UnauthorizedAccessException), "You don't have permission to perform this action." },
                { typeof(HttpRequestException), "Network connection error. Please check your internet connection." },
                { typeof(TimeoutException), "The operation took too long to complete. Please try again." },
                { typeof(SQLite.SQLiteException), "Database error occurred. Please try again or contact support." },
                { typeof(System.IO.IOException), "File operation failed. Please ensure the app has necessary permissions." },
                { typeof(ArgumentNullException), "Invalid input provided. Please check your data and try again." },
                { typeof(ArgumentException), "Invalid input provided. Please check your data and try again." },
                { typeof(InvalidOperationException), "Operation cannot be performed at this time. Please try again later." }
            };
        }

        /// <summary>
        /// Handles exceptions and shows user-friendly error messages
        /// </summary>
        public async Task<bool> HandleErrorAsync(Exception exception, string context = "", Page? page = null)
        {
            try
            {
                // Log the error
                _logger?.LogError(exception, "Error in context: {Context}", context);

                // Get user-friendly message
                var userMessage = GetUserFriendlyMessage(exception);
                var title = GetErrorTitle(exception);

                // Show error to user
                if (page != null)
                {
                    await page.DisplayAlert(title, userMessage, "OK");
                }
                else if (Application.Current?.Windows?.Count > 0)
                {
                    var mainWindow = Application.Current.Windows.FirstOrDefault();
                    var mainPage = mainWindow?.Page;
                    if (mainPage != null && mainPage is Page pageInstance)
                    {
                        await pageInstance.DisplayAlert(title, userMessage, "OK");
                    }
                }

                return true;
            }
            catch (Exception handlingException)
            {
                // Fallback error handling
                _logger?.LogCritical(handlingException, "Error occurred while handling original error in context: {Context}", context);
                System.Diagnostics.Debug.WriteLine($"Critical error in error handling: {handlingException.Message}");
                return false;
            }
        }

        /// <summary>
        /// Handles exceptions and returns user-friendly error information
        /// </summary>
        public ErrorInfo ProcessError(Exception exception, string context = "")
        {
            try
            {
                // Log the error
                _logger?.LogError(exception, "Error in context: {Context}", context);

                return new ErrorInfo
                {
                    Title = GetErrorTitle(exception),
                    Message = GetUserFriendlyMessage(exception),
                    Context = context,
                    Severity = GetErrorSeverity(exception),
                    ShowToUser = ShouldShowToUser(exception),
                    Exception = exception
                };
            }
            catch (Exception handlingException)
            {
                _logger?.LogCritical(handlingException, "Error occurred while processing error in context: {Context}", context);
                
                return new ErrorInfo
                {
                    Title = "System Error",
                    Message = "An unexpected error occurred. Please try again or contact support if the problem persists.",
                    Context = context,
                    Severity = ErrorSeverity.High,
                    ShowToUser = true,
                    Exception = exception
                };
            }
        }

        /// <summary>
        /// Shows a toast-style error message (if supported by the platform)
        /// </summary>
        public async Task ShowToastErrorAsync(string message, int durationMs = 3000)
        {
            try
            {
                // Implementation would depend on available toast libraries
                // For now, we'll use a simple approach
                _logger?.LogInformation("Toast error message: {Message}", message);
                
                // Could integrate with CommunityToolkit.Maui.Alerts here
                // await Toast.Make(message).Show();
                
                await Task.Delay(100); // Placeholder
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to show toast error message");
            }
        }

        /// <summary>
        /// Logs an error without showing it to the user
        /// </summary>
        public void LogError(Exception exception, string context = "", object? additionalData = null)
        {
            try
            {
                if (additionalData != null)
                {
                    _logger?.LogError(exception, "Error in context: {Context}, Additional Data: {@AdditionalData}", context, additionalData);
                }
                else
                {
                    _logger?.LogError(exception, "Error in context: {Context}", context);
                }
            }
            catch (Exception loggingException)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log error: {loggingException.Message}");
            }
        }

        /// <summary>
        /// Gets user-friendly error message based on exception type
        /// </summary>
        private string GetUserFriendlyMessage(Exception exception)
        {
            // Check if we have a specific message for this exception type
            var exceptionType = exception.GetType();
            if (_errorMessages.TryGetValue(exceptionType, out var message))
            {
                return message;
            }

            // Check base types
            foreach (var kvp in _errorMessages)
            {
                if (kvp.Key.IsAssignableFrom(exceptionType))
                {
                    return kvp.Value;
                }
            }

            // Check for specific error patterns in the message
            var exceptionMessage = exception.Message?.ToLower() ?? "";
            
            if (exceptionMessage.Contains("network") || exceptionMessage.Contains("connection"))
                return "Network connection error. Please check your internet connection and try again.";
            
            if (exceptionMessage.Contains("timeout"))
                return "The operation took too long to complete. Please try again.";
            
            if (exceptionMessage.Contains("permission") || exceptionMessage.Contains("unauthorized"))
                return "You don't have permission to perform this action.";
            
            if (exceptionMessage.Contains("not found"))
                return "The requested item could not be found. It may have been moved or deleted.";

            // Default message
            return "An unexpected error occurred. Please try again or contact support if the problem persists.";
        }

        /// <summary>
        /// Gets appropriate error title based on exception type
        /// </summary>
        private string GetErrorTitle(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => "Access Denied",
                HttpRequestException => "Network Error",
                TimeoutException => "Timeout Error",
                SQLite.SQLiteException => "Database Error",
                System.IO.IOException => "File Error",
                ArgumentNullException => "Invalid Input",
                ArgumentException => "Invalid Input",
                InvalidOperationException => "Operation Error",
                _ => "Error"
            };
        }

        /// <summary>
        /// Gets error severity based on exception type
        /// </summary>
        private ErrorSeverity GetErrorSeverity(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => ErrorSeverity.Medium,
                HttpRequestException => ErrorSeverity.Medium,
                TimeoutException => ErrorSeverity.Low,
                SQLite.SQLiteException => ErrorSeverity.High,
                System.IO.IOException => ErrorSeverity.Medium,
                ArgumentNullException => ErrorSeverity.Low,
                ArgumentException => ErrorSeverity.Low,
                InvalidOperationException => ErrorSeverity.Medium,
                _ => ErrorSeverity.Medium
            };
        }

        /// <summary>
        /// Determines if error should be shown to user
        /// </summary>
        private bool ShouldShowToUser(Exception exception)
        {
            // Generally show most errors to users, but hide system/internal errors
            return exception switch
            {
                System.Threading.ThreadAbortException => false,
                System.ObjectDisposedException => false,
                _ => true
            };
        }
    }

    /// <summary>
    /// Contains information about a processed error
    /// </summary>
    public class ErrorInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public ErrorSeverity Severity { get; set; }
        public bool ShowToUser { get; set; }
        public Exception? Exception { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Error severity levels
    /// </summary>
    public enum ErrorSeverity
    {
        Low,      // Minor issues, user can continue
        Medium,   // Moderate issues, operation failed but app is stable
        High,     // Serious issues, might affect app stability
        Critical  // Critical issues, app might need to restart
    }
}
