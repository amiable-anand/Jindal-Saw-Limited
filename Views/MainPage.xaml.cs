using Jindal.Services;

namespace Jindal.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                await DatabaseService.Init();

                var username = EmployeeCode.Text?.Trim();
                var password = Password.Text?.Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage.Text = "Please enter both username and password.";
                    ErrorMessage.IsVisible = true;
                    return;
                }

                var emp = await DatabaseService.GetEmployee(username, password);

                if (emp != null)
                {
                    Application.Current.MainPage = new AppShell(); // ✅ Safe
                }
                else
                {
                    ErrorMessage.Text = "Invalid credentials.";
                    ErrorMessage.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                await DisplayAlert("Login Failed", "An error occurred. Please try again.", "OK");
            }
        }

    }
}
