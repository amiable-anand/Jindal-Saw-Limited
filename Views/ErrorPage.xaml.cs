using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Logging;
using Jindal.Services;
using System;
using System.Threading.Tasks;

namespace Jindal.Views
{
    public partial class ErrorPage : ContentPage
    {
        private readonly string _originalErrorMessage;
        private readonly Exception? _originalException;
        private readonly ILogger<ErrorPage>? _logger;
        private bool _detailsVisible = false;

        public ErrorPage(string errorMessage, Exception? exception = null)
        {
            InitializeComponent();
            
            _originalErrorMessage = errorMessage;
            _originalException = exception;
            
            // Logger will be injected when available, otherwise use null
            _logger = null;
            
            InitializeErrorDisplay();
        }

        private void InitializeErrorDisplay()
        {
            ErrorMessageLabel.Text = _originalErrorMessage;
            
            if (_originalException != null)
            {
                ErrorDetailsLabel.Text = $"Technical Details:\n{_originalException.GetType().Name}: {_originalException.Message}";
                ToggleDetailsButton.IsVisible = true;
            }
            else
            {
                ToggleDetailsButton.IsVisible = false;
            }
            
            _logger?.LogError(_originalException, "ErrorPage displayed with message: {ErrorMessage}", _originalErrorMessage);
        }

        private async void OnRetryClicked(object sender, EventArgs e)
        {
            try
            {
                RetryButton.Text = "🔄 Retrying...";
                RetryButton.IsEnabled = false;
                
                _logger?.LogInformation("User clicked retry on ErrorPage");
                
                // Try to initialize database again
                await DatabaseService.Init();
                
                // Test database connection
                var isConnected = await DatabaseService.TestDatabaseConnection();
                if (!isConnected)
                {
                    throw new Exception("Database connection test failed");
                }
                
                // Navigate to main page
                await NavigateToMainPage();
                
                _logger?.LogInformation("ErrorPage retry successful, navigated to MainPage");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "ErrorPage retry failed");
                ErrorMessageLabel.Text = $"Still experiencing issues: {ex.Message}";
                
                // Update error details if new exception occurred
                if (_originalException == null)
                {
                    ErrorDetailsLabel.Text = $"Technical Details:\n{ex.GetType().Name}: {ex.Message}";
                    ToggleDetailsButton.IsVisible = true;
                }
            }
            finally
            {
                RetryButton.Text = "🔄 Try Again";
                RetryButton.IsEnabled = true;
            }
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            try
            {
                HomeButton.Text = "🏠 Going Home...";
                HomeButton.IsEnabled = false;
                
                _logger?.LogInformation("User clicked home on ErrorPage");
                
                // Clear any stored login state
                Preferences.Clear();
                
                // Navigate to main page (login)
                await NavigateToMainPage();
                
                _logger?.LogInformation("ErrorPage home navigation successful");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "ErrorPage home navigation failed");
                await DisplayAlert("Navigation Error", "Unable to navigate to login page. Please restart the app.", "OK");
            }
            finally
            {
                HomeButton.Text = "🏠 Go to Login";
                HomeButton.IsEnabled = true;
            }
        }

        private void OnToggleDetailsClicked(object sender, EventArgs e)
        {
            try
            {
                _detailsVisible = !_detailsVisible;
                ErrorDetailsLabel.IsVisible = _detailsVisible;
                ToggleDetailsButton.Text = _detailsVisible ? "Hide Details" : "Show Details";
                
                _logger?.LogDebug("ErrorPage details toggled: {DetailsVisible}", _detailsVisible);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error toggling details visibility");
            }
        }

        private async Task NavigateToMainPage()
        {
            try
            {
                // Add a small delay to ensure UI thread is ready
                await Task.Delay(100);
                
                var mainPage = new NavigationPage(new MainPage());
                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = mainPage;
                }
                else
                {
                    throw new InvalidOperationException("No application windows available");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to navigate to MainPage");
                throw;
            }
        }
    }
}
