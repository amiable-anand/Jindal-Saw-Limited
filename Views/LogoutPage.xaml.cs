namespace Jindal.Views
{
    public partial class LogoutPage : ContentPage
    {
        public LogoutPage()
        {
            InitializeComponent();
            Logout();
        }

        private void Logout()
        {
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
    }
}
