using Microsoft.Maui.Controls;

namespace Jindal
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
           MainPage = new NavigationPage(new Views.MainPage());
        }

        //protected override Window CreateWindow(IActivationState activationState)
       // {
            // ✅ Setting the initial page here
          //  return new Window(new NavigationPage(new Views.MainPage()));
      //  }

    }
}

