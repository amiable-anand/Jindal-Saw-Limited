using Jindal.Services;
using Jindal.Models;

namespace Jindal
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            DatabaseService.AddTestEmployee(); // Optional: Add dummy data
        }

        private void OnShowPasswordChanged(object sender, CheckedChangedEventArgs e)
        {
            PasswordEntry.IsPassword = !e.Value;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string code = EmployeeCodeEntry.Text?.Trim();
            string pass = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(pass))
            {
                ErrorMessage.Text = "Please enter both fields.";
                ErrorMessage.IsVisible = true;
                return;
            }

            var employee = await DatabaseService.GetEmployee(code, pass);
            if (employee != null)
            {
                ErrorMessage.IsVisible = false;
                await DisplayAlert("Success", "Login successful!", "OK");
                // TODO: Navigate to home page
            }
            else
            {
                ErrorMessage.Text = "Invalid credentials.";
                ErrorMessage.IsVisible = true;
            }
        }
    }
}
