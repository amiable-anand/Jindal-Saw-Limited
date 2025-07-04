using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Jindal
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            bool isLoggedIn = Preferences.Get("IsLoggedIn", false);

            if (isLoggedIn)
            {
                MainPage = new AppShell(); // User already logged in
            }
            else
            {
                MainPage = new NavigationPage(new Views.MainPage()); // Show login page
            }
        }
    }
}
