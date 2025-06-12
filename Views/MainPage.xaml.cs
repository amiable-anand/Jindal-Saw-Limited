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
            var employee = await DatabaseService.GetEmployee(UsernameEntry.Text, PasswordEntry.Text);

            if (employee != null)
            {
                Application.Current.MainPage = new AppShell(); // Navigate to flyout UI
            }
            else
            {
                ErrorMessage.Text = "Invalid Employee Code or Password";
                ErrorMessage.IsVisible = true;
            }
        }
    }
}
