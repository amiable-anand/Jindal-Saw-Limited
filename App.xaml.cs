using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Jindal
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            bool isLoggedIn = Preferences.Get("IsLoggedIn", false);

            Page mainPage;
            if (isLoggedIn)
            {
                mainPage = new AppShell(); // User already logged in - will show dashboard by default
            }
            else
            {
                mainPage = new NavigationPage(new Views.MainPage()); // Show login page
            }

            return new Window(mainPage)
            {
                Title = "Jindal Guest Management"
            };
        }
    }
}
