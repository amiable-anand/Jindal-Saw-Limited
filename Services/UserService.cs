using SQLite;
using Jindal.Models;
using BCrypt.Net;
using Microsoft.Maui.Storage;

namespace Jindal.Services
{
    public class UserService
    {
        private readonly SQLiteAsyncConnection _database;
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
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "JindalGuestManagement.db");
            _database = new SQLiteAsyncConnection(dbPath);
        }

        private static bool _isInitialized = false;
        private static readonly object _initLock = new();

        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized) return true;
            
            lock (_initLock)
            {
                if (_isInitialized) return true;
                _isInitialized = true;
            }

            try
            {
                await _database.CreateTableAsync<User>();
                await CreateDefaultAdminUser();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                _isInitialized = false;
                return false;
            }
        }

        private async Task CreateDefaultAdminUser()
        {
            var existingAdmin = await _database.Table<User>()
                .Where(u => u.Role == UserRole.Admin)
                .FirstOrDefaultAsync();

            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    FullName = "System Administrator",
                    Email = "admin@jindal.com",
                    Permissions = (int)Permission.All,
                    IsActive = true
                };

                await _database.InsertAsync(adminUser);
            }
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            try
            {
                await InitializeAsync();
                
                var user = await _database.Table<User>()
                    .Where(u => u.Username == username && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    user.LastLoginAt = DateTime.Now;
                    await _database.UpdateAsync(user);
                    
                    // Store current user info in preferences
                    Preferences.Set("IsLoggedIn", true);
                    Preferences.Set("CurrentUserId", user.Id);
                    Preferences.Set("CurrentUserRole", (int)user.Role);
                    Preferences.Set("CurrentUserPermissions", user.Permissions);
                    Preferences.Set("CurrentUserFullName", user.FullName);
                    
                    return user;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Authentication error: {ex.Message}");
            }
            
            return null;
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                Preferences.Clear();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            await InitializeAsync();
            return await _database.Table<User>().ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _database.Table<User>()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                await InitializeAsync();
                
                // Hash password before saving
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                await _database.InsertAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create user error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                await _database.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update user error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user != null)
                {
                    await _database.DeleteAsync(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete user error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserPermissionsAsync(int userId, int permissions)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.Permissions = permissions;
                    await _database.UpdateAsync(user);
                    return true;
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
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user != null && BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    await _database.UpdateAsync(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Change password error: {ex.Message}");
                return false;
            }
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
