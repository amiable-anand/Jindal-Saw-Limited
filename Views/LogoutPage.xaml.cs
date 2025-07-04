using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

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
                // ?? Clear stored login/session data
                Preferences.Set("IsLoggedIn", false);
                Preferences.Remove("UserId");
                Preferences.Remove("UserName");

                // ?? Navigate to MainPage and remove history
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                // Cancel logout, return to previous page
                await Navigation.PopAsync();
            }
        }
    }
}
