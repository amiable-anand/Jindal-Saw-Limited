using Jindal.Services;
using Microsoft.Maui.Controls;

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
            var emp = await DatabaseService.GetEmployee(UsernameEntry.Text, PasswordEntry.Text);
            if (emp != null)
            {
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//RoomPage");
            }
            else
            {
                ErrorMessage.Text = "Invalid credentials.";
                ErrorMessage.IsVisible = true;
            }
        }
    }
}
