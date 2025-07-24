using Jindal.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using Jindal.Models;
using Microsoft.Extensions.Logging;

namespace Jindal.Views
{
    public partial class MainPage : ContentPage
    {
        private bool _isLoggingIn = false;
        public MainPage()
        {
            InitializeComponent();
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Hide error messages initially
            ErrorMessage.IsVisible = false;
            UsernameValidationMessage.IsVisible = false;
            PasswordValidationMessage.IsVisible = false;
            
            // Set initial API status
            UpdateApiStatus(true); // Assume connected initially
        }

        private void UpdateApiStatus(bool isConnected)
        {
            ApiStatusIndicator.Fill = isConnected ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444");
            ApiStatusLabel.Text = isConnected ? "System Ready" : "System Offline";
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

                // 👤 Authenticate user using unified DatabaseService
                var user = await DatabaseService.AuthenticateUser(username, password);

                if (user != null)
                {
                    // Store current user info in preferences
                    Preferences.Set("IsLoggedIn", true);
                    Preferences.Set("CurrentUserId", user.Id);
                    Preferences.Set("CurrentUserRole", (int)user.Role);
                    Preferences.Set("CurrentUserPermissions", user.Permissions);
                    Preferences.Set("CurrentUserFullName", user.FullName);
                    Preferences.Set("CurrentUserUsername", user.Username);
                    
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
        /// Handles username text changes for validation.
        /// </summary>
        private void OnUsernameTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput();
        }

        /// <summary>
        /// Handles password text changes for validation.
        /// </summary>
        private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput();
        }

        /// <summary>
        /// Validates input and enables/disables login button.
        /// </summary>
        private void ValidateInput()
        {
            var username = UsernameEntry.Text?.Trim();
            var password = Password.Text?.Trim();
            
            // Reset validation messages
            UsernameValidationMessage.IsVisible = false;
            PasswordValidationMessage.IsVisible = false;
            
            // Validate username
            if (string.IsNullOrWhiteSpace(username))
            {
                UsernameValidationMessage.Text = "Username is required";
                UsernameValidationMessage.IsVisible = true;
            }
            else if (username.Length < 3)
            {
                UsernameValidationMessage.Text = "Username must be at least 3 characters";
                UsernameValidationMessage.IsVisible = true;
            }
            
            // Validate password
            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordValidationMessage.Text = "Password is required";
                PasswordValidationMessage.IsVisible = true;
            }
            else if (password.Length < 4)
            {
                PasswordValidationMessage.Text = "Password must be at least 4 characters";
                PasswordValidationMessage.IsVisible = true;
            }
            
            // Enable login button only if both fields are valid
            LoginButton.IsEnabled = !string.IsNullOrWhiteSpace(username) && 
                                   username.Length >= 3 && 
                                   !string.IsNullOrWhiteSpace(password) && 
                                   password.Length >= 4;
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
