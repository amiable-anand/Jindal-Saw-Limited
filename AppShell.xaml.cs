using Jindal.Views;
using Microsoft.Maui.Controls;

namespace Jindal
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("AddEditRoomPage", typeof(AddEditRoomPage));
        }
    }
}
