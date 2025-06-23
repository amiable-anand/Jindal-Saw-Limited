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
            var emp = await DatabaseService.GetEmployee(EmployeeCode.Text, Password.Text);
            if (emp != null)
            {
                // Application.Current.MainPage = new AppShell();
                Application.Current.Windows[0].Page = new AppShell(); // ✅ after successful login

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
