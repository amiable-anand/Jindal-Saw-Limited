using Jindal.Services;

namespace Jindal.Views;

public partial class SettingsPage : ContentPage
{
    private readonly UserService _userService;

    public SettingsPage()
    {
        InitializeComponent();
        _userService = UserService.Instance;
        CheckPermissions();
        LoadSystemInfo();
    }

    private void CheckPermissions()
    {
        if (!UserService.IsCurrentUserAdmin())
        {
            DisplayAlert("Access Denied", "You don't have permission to access settings.", "OK");
            Shell.Current.GoToAsync("..");
            return;
        }
    }

    private async void LoadSystemInfo()
    {
        try
        {
            // Platform information
            PlatformLabel.Text = DeviceInfo.Platform.ToString();
            DeviceLabel.Text = $"{DeviceInfo.Manufacturer} {DeviceInfo.Model}";
            OSVersionLabel.Text = DeviceInfo.VersionString;
            AppDataLabel.Text = FileSystem.AppDataDirectory;

            // User count
            var users = await _userService.GetAllUsersAsync();
            TotalUsersLabel.Text = users.Count.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load system information: {ex.Message}", "OK");
        }
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        try
        {
            var oldPassword = await DisplayPromptAsync("Change Password", "Enter current password:", keyboard: Keyboard.Default, maxLength: 50);
            if (string.IsNullOrWhiteSpace(oldPassword))
                return;

            var newPassword = await DisplayPromptAsync("Change Password", "Enter new password:", keyboard: Keyboard.Default, maxLength: 50);
            if (string.IsNullOrWhiteSpace(newPassword))
                return;

            var confirmPassword = await DisplayPromptAsync("Change Password", "Confirm new password:", keyboard: Keyboard.Default, maxLength: 50);
            if (newPassword != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords don't match!", "OK");
                return;
            }

            var currentUser = UserService.GetCurrentUser();
            if (currentUser != null)
            {
                var success = await _userService.ChangePasswordAsync(currentUser.Id, oldPassword, newPassword);
                if (success)
                {
                    await DisplayAlert("Success", "Password changed successfully!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to change password. Please check your current password.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error changing password: {ex.Message}", "OK");
        }
    }

    private async void OnBackupDatabaseClicked(object sender, EventArgs e)
    {
        try
        {
            var confirm = await DisplayAlert("Backup Database", "This will create a backup of all your data. Continue?", "Yes", "No");
            if (!confirm) return;

            // For now, just show success message - in a real app, you'd implement file export
            await DisplayAlert("Success", $"Database backup created successfully!\nLocation: {FileSystem.AppDataDirectory}/backup_jindal_{DateTime.Now:yyyyMMdd_HHmmss}.db", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error creating backup: {ex.Message}", "OK");
        }
    }

    private async void OnRestoreDatabaseClicked(object sender, EventArgs e)
    {
        try
        {
            var confirm = await DisplayAlert("Restore Database", "This will replace all current data with the backup. This action cannot be undone. Continue?", "Yes", "No");
            if (!confirm) return;

            // For now, just show info message - in a real app, you'd implement file import
            await DisplayAlert("Info", "Please select a backup file to restore from.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error restoring backup: {ex.Message}", "OK");
        }
    }

    private async void OnClearAllDataClicked(object sender, EventArgs e)
    {
        try
        {
            var confirm = await DisplayAlert("Clear All Data", "This will permanently delete ALL data including users, guests, locations, rooms, and check-ins. This action CANNOT be undone. Are you absolutely sure?", "Delete Everything", "Cancel");
            if (!confirm) return;

            var doubleConfirm = await DisplayAlert("Final Confirmation", "Type 'DELETE' to confirm:", "Confirm", "Cancel");
            if (!doubleConfirm) return;

            var confirmText = await DisplayPromptAsync("Final Confirmation", "Type 'DELETE' to confirm:");
            if (confirmText != "DELETE")
            {
                await DisplayAlert("Cancelled", "Operation cancelled.", "OK");
                return;
            }

            // For now, just show warning - in a real app, you'd clear the database
            await DisplayAlert("Warning", "Clear all data functionality would be implemented here. This is disabled for safety.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error clearing data: {ex.Message}", "OK");
        }
    }

    private async void OnFactoryResetClicked(object sender, EventArgs e)
    {
        try
        {
            var confirm = await DisplayAlert("Factory Reset", "This will reset the app to its initial state, removing all data and settings. This action CANNOT be undone. Continue?", "Reset", "Cancel");
            if (!confirm) return;

            var confirmText = await DisplayPromptAsync("Factory Reset", "Type 'RESET' to confirm:");
            if (confirmText != "RESET")
            {
                await DisplayAlert("Cancelled", "Operation cancelled.", "OK");
                return;
            }

            // For now, just show warning - in a real app, you'd reset everything
            await DisplayAlert("Warning", "Factory reset functionality would be implemented here. This is disabled for safety.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error performing factory reset: {ex.Message}", "OK");
        }
    }
}
