using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Jindal.Views
{
    public partial class LogoutPage : ContentPage
    {
        public LogoutPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "Cancel");

            if (confirm)
            {
                // ? Optional: clear stored login/session data
                // Preferences.Default.Remove("IsLoggedIn");

                // ? Navigate to MainPage and remove navigation history
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                // Cancel logout: go back to previous screen
                await Navigation.PopAsync();
            }
        }
    }
}
