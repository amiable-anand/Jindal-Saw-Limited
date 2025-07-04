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
                MainPage = new AppShell(); // Home shell
            else
                MainPage = new NavigationPage(new Views.MainPage()); // Login page
        }
    }
}
