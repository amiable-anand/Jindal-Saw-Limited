using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Jindal.Views;

public partial class UserManagementPage : ContentPage
{
    private readonly UserService _userService;

    public UserManagementPage()
    {
        InitializeComponent();
        _userService = UserService.Instance;
        CheckPermissions();
    }

    private void CheckPermissions()
    {
        if (!UserService.IsCurrentUserAdmin())
        {
            DisplayAlert("Access Denied", "You don't have permission to access user management.", "OK");
            Shell.Current.GoToAsync("..");
            return;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUsers();
    }

    private async Task LoadUsers()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            UsersContainer.Children.Clear();

            var users = await _userService.GetAllUsersAsync();

            foreach (var user in users)
            {
                var userCard = CreateUserCard(user);
                UsersContainer.Children.Add(userCard);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private Border CreateUserCard(User user)
    {
        var border = new Border
        {
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = 16,
            Margin = new Thickness(0, 8),
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Opacity = 0.1f,
                Radius = 4,
                Offset = new Point(0, 2)
            }
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        // User Info Section
        var userInfoStack = new StackLayout();

        var nameLabel = new Label
        {
            Text = $"👤 {user.FullName}",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#1E3A8A")
        };

        var usernameLabel = new Label
        {
            Text = $"@{user.Username}",
            FontSize = 14,
            TextColor = Color.FromArgb("#6B7280")
        };

        var roleLabel = new Label
        {
            Text = $"Role: {user.Role}",
            FontSize = 14,
            TextColor = user.Role == UserRole.Admin ? Color.FromArgb("#DC2626") : Color.FromArgb("#059669"),
            FontAttributes = FontAttributes.Bold
        };

        var permissionsLabel = new Label
        {
            Text = $"Permissions: {user.GetPermissionDisplayText()}",
            FontSize = 12,
            TextColor = Color.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var statusLabel = new Label
        {
            Text = user.IsActive ? "✅ Active" : "❌ Inactive",
            FontSize = 12,
            TextColor = user.IsActive ? Color.FromArgb("#059669") : Color.FromArgb("#DC2626")
        };

        var lastLoginLabel = new Label
        {
            Text = user.LastLoginAt.HasValue 
                ? $"Last login: {user.LastLoginAt.Value:MMM dd, yyyy HH:mm}"
                : "Never logged in",
            FontSize = 11,
            TextColor = Color.FromArgb("#9CA3AF")
        };

        userInfoStack.Children.Add(nameLabel);
        userInfoStack.Children.Add(usernameLabel);
        userInfoStack.Children.Add(roleLabel);
        userInfoStack.Children.Add(permissionsLabel);
        userInfoStack.Children.Add(statusLabel);
        userInfoStack.Children.Add(lastLoginLabel);

        // Actions Section with ScrollView for horizontal scrolling
        var actionsScrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            VerticalOptions = LayoutOptions.Center
        };
        
        var actionsStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 8
        };

        var editButton = new Button
        {
            Text = "✏️ Edit",
            BackgroundColor = Color.FromArgb("#E5E7EB"),
            TextColor = Color.FromArgb("#374151"),
            CornerRadius = 8,
            FontSize = 12,
            Padding = new Thickness(12, 6),
            HeightRequest = 36,
            WidthRequest = 80
        };
        editButton.Clicked += async (s, e) => await OnEditUserClicked(user);

        var permissionsButton = new Button
        {
            Text = "🔐 Permissions",
            BackgroundColor = Color.FromArgb("#FEF3C7"),
            TextColor = Color.FromArgb("#92400E"),
            CornerRadius = 8,
            FontSize = 12,
            Padding = new Thickness(12, 6),
            HeightRequest = 36,
            WidthRequest = 110
        };
        permissionsButton.Clicked += async (s, e) => await OnManagePermissionsClicked(user);

        var deleteButton = new Button
        {
            Text = "🗑️ Delete",
            BackgroundColor = Color.FromArgb("#DC2626"),
            TextColor = Colors.White,
            CornerRadius = 8,
            FontSize = 12,
            Padding = new Thickness(12, 6),
            HeightRequest = 36,
            WidthRequest = 80
        };
        deleteButton.Clicked += async (s, e) => await OnDeleteUserClicked(user);

        actionsStack.Children.Add(editButton);
        if (user.Role != UserRole.Admin) // Don't allow changing admin permissions
        {
            actionsStack.Children.Add(permissionsButton);
        }
        actionsStack.Children.Add(deleteButton);
        
        // Add the actions stack to the scroll view
        actionsScrollView.Content = actionsStack;

        grid.Add(userInfoStack, 0, 0);
        grid.Add(actionsScrollView, 1, 0);

        border.Content = grid;
        return border;
    }

    private async void OnAddUserClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("AddEditUserPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation Error", $"Could not open add user page: {ex.Message}", "OK");
        }
    }

    private async Task OnEditUserClicked(User user)
    {
        try
        {
            await Shell.Current.GoToAsync($"AddEditUserPage?userId={user.Id}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation Error", $"Could not open edit user page: {ex.Message}", "OK");
        }
    }

    private async Task OnManagePermissionsClicked(User user)
    {
        var permissionOptions = new List<string>();
        var permissions = Enum.GetValues<Permission>().Where(p => p != Permission.None && p != Permission.All).ToArray();
        
        foreach (var permission in permissions)
        {
            var hasPermission = user.HasPermission(permission);
            permissionOptions.Add($"{(hasPermission ? "✅" : "❌")} {permission}");
        }

        var result = await DisplayActionSheet(
            $"Manage Permissions for {user.FullName}",
            "Cancel",
            null,
            permissionOptions.ToArray()
        );

        if (result != null && result != "Cancel")
        {
            var selectedPermission = permissions[Array.IndexOf(permissionOptions.ToArray(), result)];
            await TogglePermission(user, selectedPermission);
        }
    }

    private async Task TogglePermission(User user, Permission permission)
    {
        try
        {
            if (user.HasPermission(permission))
            {
                user.RevokePermission(permission);
            }
            else
            {
                user.GrantPermission(permission);
            }

            var success = await _userService.UpdateUserPermissionsAsync(user.Id, user.Permissions);
            if (success)
            {
                await DisplayAlert("Success", $"Permission updated for {user.FullName}", "OK");
                await LoadUsers(); // Refresh the list
            }
            else
            {
                await DisplayAlert("Error", "Failed to update permissions", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error updating permissions: {ex.Message}", "OK");
        }
    }

    private async Task OnDeleteUserClicked(User user)
    {
        if (user.Role == UserRole.Admin)
        {
            await DisplayAlert("Cannot Delete", "Cannot delete admin users for security reasons.", "OK");
            return;
        }

        var confirm = await DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete user '{user.FullName}'? This action cannot be undone.",
            "Delete",
            "Cancel"
        );

        if (confirm)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(user.Id);
                if (success)
                {
                    await DisplayAlert("Success", "User deleted successfully", "OK");
                    await LoadUsers(); // Refresh the list
                }
                else
                {
                    await DisplayAlert("Error", "Failed to delete user", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error deleting user: {ex.Message}", "OK");
            }
        }
    }
}
