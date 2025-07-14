using Jindal.Views;
using Jindal.Services;
using Jindal.Models;
using Microsoft.Maui.Storage;

namespace Jindal;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Setup role-based navigation
        SetupUserInterface();

        // Register hidden or additional navigation routes
        RegisterRoutes();
        
        // Clear navigation context on app start
        NavigationService.ClearContext();
        
        // Navigate to Dashboard as default page
        _ = GoToAsync("//DashboardPage");
    }

    private void SetupUserInterface()
    {
        var currentUser = GetCurrentUser();
        if (currentUser != null)
        {
            // Update welcome message
            WelcomeLabel.Text = $"Welcome, {currentUser.FullName}!";
            RoleLabel.Text = $"Role: {currentUser.Role}";

            // Hide/Show menu items based on permissions
            SetupMenuVisibility(currentUser);
        }
    }

    private void SetupMenuVisibility(User currentUser)
    {
        // Admin gets access to everything
        if (currentUser.Role == UserRole.Admin)
        {
            return; // Admin can see all items
        }

        // Hide admin-only items for normal users
        UserManagementItem.IsVisible = false;
        SettingsItem.IsVisible = false;

        // Hide items based on permissions
        LocationsItem.IsVisible = currentUser.HasPermission(Permission.LocationManagement);
        RoomsItem.IsVisible = currentUser.HasPermission(Permission.RoomManagement);
        CheckInOutItem.IsVisible = currentUser.HasPermission(Permission.CheckInOut) || 
                                   currentUser.HasPermission(Permission.AddGuest) || 
                                   currentUser.HasPermission(Permission.DeleteGuest);
        ReportsItem.IsVisible = currentUser.HasPermission(Permission.ReportAccess);
    }

    private void RegisterRoutes()
    {
        // Main Pages
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
        Routing.RegisterRoute(nameof(LocationPage), typeof(LocationPage));
        Routing.RegisterRoute(nameof(RoomPage), typeof(RoomPage));
        Routing.RegisterRoute(nameof(CheckInOutPage), typeof(CheckInOutPage));
        Routing.RegisterRoute(nameof(ReportPage), typeof(ReportPage));
        Routing.RegisterRoute(nameof(LogoutPage), typeof(LogoutPage));

        // Admin Pages
        Routing.RegisterRoute(nameof(UserManagementPage), typeof(UserManagementPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

        // Modal/Detail Pages
        Routing.RegisterRoute(nameof(AddCheckInOutPage), typeof(AddCheckInOutPage));
        Routing.RegisterRoute(nameof(AddEditLocationPage), typeof(AddEditLocationPage));
        Routing.RegisterRoute(nameof(AddEditRoomPage), typeof(AddEditRoomPage));
        Routing.RegisterRoute(nameof(AddGuestToSameRoomPage), typeof(AddGuestToSameRoomPage));
        Routing.RegisterRoute(nameof(EditGuestPage), typeof(EditGuestPage));
        Routing.RegisterRoute(nameof(CheckOutPage), typeof(CheckOutPage));
        Routing.RegisterRoute(nameof(AddEditUserPage), typeof(AddEditUserPage));
    }

    public void RefreshUserInterface()
    {
        SetupUserInterface();
    }

    // Helper method to get current user from preferences
    private static User? GetCurrentUser()
    {
        if (!Preferences.Get("IsLoggedIn", false))
            return null;

        return new User
        {
            Id = Preferences.Get("CurrentUserId", 0),
            Role = (UserRole)Preferences.Get("CurrentUserRole", (int)UserRole.Normal),
            Permissions = Preferences.Get("CurrentUserPermissions", 0),
            FullName = Preferences.Get("CurrentUserFullName", "Unknown User"),
            Username = Preferences.Get("CurrentUserUsername", "Unknown")
        };
    }

    public static bool HasCurrentUserPermission(Permission permission)
    {
        var currentUser = GetCurrentUser();
        return currentUser?.HasPermission(permission) ?? false;
    }

    public static bool IsCurrentUserAdmin()
    {
        var currentUser = GetCurrentUser();
        return currentUser?.Role == UserRole.Admin;
    }
}
