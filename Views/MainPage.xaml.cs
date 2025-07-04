using Jindal.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;

namespace Jindal.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles login button click: validates credentials and navigates to AppShell if successful.
        /// </summary>
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                // Initialize database (if not already)
                await DatabaseService.Init();

                var username = EmployeeCode.Text?.Trim();
                var password = Password.Text?.Trim();

                // 🔐 Input validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage.Text = "Please enter both username and password.";
                    ErrorMessage.IsVisible = true;
                    return;
                }

                // 👤 Authenticate user
                var emp = await DatabaseService.GetEmployee(username, password);

                if (emp != null)
                {
                    // ✅ Save login status and user info
                    Preferences.Set("IsLoggedIn", true);
                    Preferences.Set("UserId", emp.Id);
                    Preferences.Set("UserCode", emp.EmployeeCode); // Use this instead of Name

                    // ✅ Navigate to AppShell (home page)
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    // ❌ Invalid login
                    ErrorMessage.Text = "Invalid credentials.";
                    ErrorMessage.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                // ⚠️ Log and show error
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                await DisplayAlert("Login Failed", "An error occurred. Please try again.", "OK");
            }
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
