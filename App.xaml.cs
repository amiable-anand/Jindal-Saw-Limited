using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Jindal.Services;

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
            var window = new Window(new Views.LoadingPage())
            {
                Title = "Jindal Guest Management"
            };
            
            // Initialize database and then navigate to appropriate page
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Initialize database
                    await DatabaseService.Init();
                    
                    // Check if user is logged in
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
                    
                    window.Page = mainPage;
                }
                catch (Exception ex)
                {
                    // If database initialization fails, show error page
                    System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                    window.Page = new Views.ErrorPage(ex.Message);
                }
            });
            
            return window;
        }
    }
}
