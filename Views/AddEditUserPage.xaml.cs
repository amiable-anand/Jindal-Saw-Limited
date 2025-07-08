using Jindal.Models;
using Jindal.Services;

namespace Jindal.Views;

[QueryProperty(nameof(UserId), "userId")]
public partial class AddEditUserPage : ContentPage
{
    private readonly UserService _userService;
    private User? _currentUser;
    private readonly Dictionary<Permission, CheckBox> _permissionCheckBoxes = new();
    private bool _isSaving = false;
    
    public string? UserId { get; set; }

    public AddEditUserPage()
    {
        try
        {
            InitializeComponent();
            _userService = UserService.Instance;
            
            if (!CheckPermissions())
                return;
                
            SetupRolePicker();
            SetupPermissions();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddEditUserPage constructor error: {ex.Message}");
            _ = DisplayAlert("Error", "Failed to initialize user management page", "OK");
        }
    }

    private bool CheckPermissions()
    {
        if (!UserService.IsCurrentUserAdmin())
        {
            Dispatcher.Dispatch(async () =>
            {
                await DisplayAlert("Access Denied", "You don't have permission to manage users.", "OK");
                await Shell.Current.GoToAsync("..");
            });
            return false;
        }
        return true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserData();
    }

    private void SetupRolePicker()
    {
        RolePicker.Items.Clear();
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            RolePicker.Items.Add(role.ToString());
        }
        RolePicker.SelectedIndex = 1; // Default to Normal
    }

    private void SetupPermissions()
    {
        PermissionsContainer.Children.Clear();
        _permissionCheckBoxes.Clear();

        var permissions = Enum.GetValues<Permission>()
            .Where(p => p != Permission.None && p != Permission.All)
            .ToArray();

        foreach (var permission in permissions)
        {
            var stackLayout = new HorizontalStackLayout
            {
                Spacing = 12
            };

            var checkBox = new CheckBox
            {
                Color = Color.FromArgb("#1E3A8A")
            };

            var label = new Label
            {
                Text = GetPermissionDisplayName(permission),
                TextColor = Color.FromArgb("#374151"),
                VerticalOptions = LayoutOptions.Center
            };

            stackLayout.Children.Add(checkBox);
            stackLayout.Children.Add(label);

            PermissionsContainer.Children.Add(stackLayout);
            _permissionCheckBoxes[permission] = checkBox;
        }
    }

    private string GetPermissionDisplayName(Permission permission)
    {
        return permission switch
        {
            Permission.AddGuest => "➕ Add Guest",
            Permission.DeleteGuest => "🗑️ Delete Guest",
            Permission.CheckInOut => "🚪 Check In/Out",
            Permission.ReportAccess => "📋 Report Access",
            Permission.LocationManagement => "📍 Location Management",
            Permission.RoomManagement => "🏠 Room Management",
            Permission.UserManagement => "👥 User Management",
            _ => permission.ToString()
        };
    }

    private async Task LoadUserData()
    {
        try
        {
            if (!string.IsNullOrEmpty(UserId) && int.TryParse(UserId, out int userId))
            {
                _currentUser = await _userService.GetUserByIdAsync(userId);
                if (_currentUser != null)
                {
                    HeaderLabel.Text = "Edit User";
                    SaveButton.Text = "💾 Update User";
                    
                    // Fill form with user data
                    FullNameEntry.Text = _currentUser.FullName;
                    UsernameEntry.Text = _currentUser.Username;
                    EmailEntry.Text = _currentUser.Email;
                    IsActiveCheckBox.IsChecked = _currentUser.IsActive;
                    
                    // Hide password field for editing
                    PasswordSection.IsVisible = false;
                    
                    // Set role
                    RolePicker.SelectedIndex = (int)_currentUser.Role - 1;
                    
                    // Set permissions
                    SetPermissionCheckboxes(_currentUser.Permissions);
                }
            }
            else
            {
                _currentUser = null;
                HeaderLabel.Text = "Add New User";
                SaveButton.Text = "💾 Save User";
                PasswordSection.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load user data: {ex.Message}", "OK");
        }
    }

    private void SetPermissionCheckboxes(int permissions)
    {
        foreach (var kvp in _permissionCheckBoxes)
        {
            kvp.Value.IsChecked = (permissions & (int)kvp.Key) != 0;
        }
    }

    private int GetSelectedPermissions()
    {
        int permissions = 0;
        foreach (var kvp in _permissionCheckBoxes)
        {
            if (kvp.Value.IsChecked)
            {
                permissions |= (int)kvp.Key;
            }
        }
        return permissions;
    }

    private void OnRoleChanged(object sender, EventArgs e)
    {
        var selectedRole = (UserRole)(RolePicker.SelectedIndex + 1);
        
        // Hide permissions section for Admin users
        PermissionsSection.IsVisible = selectedRole != UserRole.Admin;
        
        if (selectedRole == UserRole.Admin)
        {
            // Check all permissions for admin
            foreach (var checkBox in _permissionCheckBoxes.Values)
            {
                checkBox.IsChecked = true;
            }
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_isSaving) return;
        
        try
        {
            _isSaving = true;
            ErrorLabel.IsVisible = false;
            
            // Update UI to show saving state
            SaveButton.Text = "🔄 Saving...";
            SaveButton.IsEnabled = false;

            // Comprehensive input validation
            if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
            {
                ShowError("Please enter a full name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
            {
                ShowError("Please enter a username.");
                return;
            }
            
            // Username validation
            if (UsernameEntry.Text.Trim().Length < 3)
            {
                ShowError("Username must be at least 3 characters long.");
                return;
            }

            if (_currentUser == null && string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                ShowError("Please enter a password for the new user.");
                return;
            }
            
            // Password validation for new users
            if (_currentUser == null && PasswordEntry.Text.Trim().Length < 6)
            {
                ShowError("Password must be at least 6 characters long.");
                return;
            }
            
            // Role validation
            if (RolePicker.SelectedIndex < 0)
            {
                ShowError("Please select a role for the user.");
                return;
            }

            var selectedRole = (UserRole)(RolePicker.SelectedIndex + 1);
            var permissions = selectedRole == UserRole.Admin ? (int)Permission.All : GetSelectedPermissions();
            
            // Ensure normal users have at least one permission
            if (selectedRole == UserRole.Normal && permissions == 0)
            {
                ShowError("Please select at least one permission for normal users.");
                return;
            }

            if (_currentUser == null)
            {
                // Create new user
                var newUser = new User
                {
                    FullName = FullNameEntry.Text.Trim(),
                    Username = UsernameEntry.Text.Trim().ToLower(),
                    Email = EmailEntry.Text?.Trim() ?? "",
                    Password = PasswordEntry.Text.Trim(),
                    Role = selectedRole,
                    Permissions = permissions,
                    IsActive = IsActiveCheckBox.IsChecked,
                    CreatedAt = DateTime.Now
                };

                var success = await _userService.CreateUserAsync(newUser);
                if (success)
                {
                    await DisplayAlert("Success", "User created successfully!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ShowError("Failed to create user. Username might already exist or there was a database error.");
                }
            }
            else
            {
                // Update existing user
                _currentUser.FullName = FullNameEntry.Text.Trim();
                _currentUser.Username = UsernameEntry.Text.Trim().ToLower();
                _currentUser.Email = EmailEntry.Text?.Trim() ?? "";
                _currentUser.Role = selectedRole;
                _currentUser.Permissions = permissions;
                _currentUser.IsActive = IsActiveCheckBox.IsChecked;

                var success = await _userService.UpdateUserAsync(_currentUser);
                if (success)
                {
                    await DisplayAlert("Success", "User updated successfully!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ShowError("Failed to update user. There might be a database error.");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Save user error: {ex.Message}");
            ShowError($"Error saving user: {ex.Message}");
        }
        finally
        {
            _isSaving = false;
            SaveButton.Text = _currentUser == null ? "💾 Save User" : "💾 Update User";
            SaveButton.IsEnabled = true;
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
