using Jindal.Views;

namespace Jindal;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register hidden or additional navigation routes
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(AddCheckInOutPage), typeof(AddCheckInOutPage));
        Routing.RegisterRoute(nameof(RoomPage), typeof(RoomPage));
       Routing.RegisterRoute(nameof(CheckInOutPage), typeof(CheckInOutPage));
       Routing.RegisterRoute(nameof(AddGuestToSameRoomPage), typeof(AddGuestToSameRoomPage));


    }
}
