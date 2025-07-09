using SQLite;
using Jindal.Models;
using BCrypt.Net;
using Microsoft.Maui.Storage;

namespace Jindal.Services
{
    public class UserService
    {
        private static UserService? _instance;
        private static readonly object _lock = new();

        public static UserService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new UserService();
                    }
                }
                return _instance;
            }
        }

        private UserService()
        {
            // Now using DatabaseService for all database operations
        }

        public async Task<bool> InitializeAsync()
        {
            // Delegate to DatabaseService for initialization
            try
            {
                await DatabaseService.Init();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                return false;
            }
        }

        // This method is now handled by DatabaseService.EnsureDefaultUserAsync()
        [Obsolete("Use DatabaseService.EnsureDefaultUserAsync() instead")]
        private async Task CreateDefaultAdminUser()
        {
            // This is now handled in DatabaseService
            await Task.CompletedTask;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            // Delegate to DatabaseService
            return await DatabaseService.AuthenticateUser(username, password);
        }

        public Task<bool> LogoutAsync()
        {
            try
            {
                Preferences.Clear();
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await DatabaseService.GetAllUsers();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await DatabaseService.GetUserById(id);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            return await DatabaseService.CreateUser(user);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            return await DatabaseService.UpdateUser(user);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await DatabaseService.DeleteUser(userId);
        }

        public async Task<bool> UpdateUserPermissionsAsync(int userId, int permissions)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.Permissions = permissions;
                    return await DatabaseService.UpdateUser(user);
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update permissions error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            return await DatabaseService.ChangePassword(userId, oldPassword, newPassword);
        }

        // Helper methods for current user
        public static User? GetCurrentUser()
        {
            if (!Preferences.Get("IsLoggedIn", false))
                return null;

            return new User
            {
                Id = Preferences.Get("CurrentUserId", 0),
                Role = (UserRole)Preferences.Get("CurrentUserRole", (int)UserRole.Normal),
                Permissions = Preferences.Get("CurrentUserPermissions", 0),
                FullName = Preferences.Get("CurrentUserFullName", "Unknown User")
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
}
