using Jindal.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;

namespace Jindal.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly UserService _userService;
        private bool _isLoggingIn = false;

        public MainPage()
        {
            InitializeComponent();
            _userService = UserService.Instance;
            
            // Hide error message initially
            ErrorMessage.IsVisible = false;
        }

        /// <summary>
        /// Handles login button click: validates credentials and navigates to AppShell if successful.
        /// </summary>
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (_isLoggingIn) return;
            
            try
            {
                _isLoggingIn = true;
                var button = sender as Button;
                if (button != null)
                {
                    button.Text = "🔄 Logging in...";
                    button.IsEnabled = false;
                }

                var username = UsernameEntry.Text?.Trim();
                var password = Password.Text?.Trim();

                // 🔐 Input validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Please enter both username and password.");
                    return;
                }

                // 👤 Authenticate user
                var user = await _userService.AuthenticateAsync(username, password);

                if (user != null)
                {
                    // ✅ Navigate to Dashboard (home page)
                    if (Application.Current?.Windows.Count > 0)
                    {
                        var appShell = new AppShell();
                        Application.Current.Windows[0].Page = appShell;
                        await Shell.Current.GoToAsync("//DashboardPage");
                    }
                }
                else
                {
                    // ❌ Invalid login
                    ShowError("Invalid username or password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                // ⚠️ Log and show error
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                ShowError("An error occurred during login. Please try again.");
            }
            finally
            {
                _isLoggingIn = false;
                var button = sender as Button;
                if (button != null)
                {
                    button.Text = "🔐 Login";
                    button.IsEnabled = true;
                }
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.TextColor = Color.FromArgb("#DC2626");
            ErrorMessage.IsVisible = true;
            
            // Hide error after 5 seconds
            Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                ErrorMessage.IsVisible = false;
                return false;
            });
        }

        /// <summary>
        /// Toggles visibility of the password field based on CheckBox.
        /// </summary>
        private void OnShowPasswordCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Password.IsPassword = !e.Value;
        }
    }
}
