using Microsoft.Maui.Controls;
using Jindal.Services;

namespace Jindal.Views
{
    public partial class ErrorPage : ContentPage
    {
        public ErrorPage(string errorMessage)
        {
            InitializeComponent();
            ErrorMessageLabel.Text = errorMessage;
        }

        private async void OnRetryClicked(object sender, EventArgs e)
        {
            try
            {
                RetryButton.Text = "🔄 Retrying...";
                RetryButton.IsEnabled = false;
                
                // Try to initialize database again
                await DatabaseService.Init();
                
                // Navigate to main page
                var mainPage = new NavigationPage(new Views.MainPage());
                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = mainPage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessageLabel.Text = $"Still experiencing issues: {ex.Message}";
            }
            finally
            {
                RetryButton.Text = "🔄 Retry";
                RetryButton.IsEnabled = true;
            }
        }
    }
}
