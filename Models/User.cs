using SQLite;
using System.ComponentModel.DataAnnotations;

namespace Jindal.Models
{
    public enum UserRole
    {
        Admin = 1,
        Normal = 2
    }

    public enum Permission
    {
        None = 0,
        AddGuest = 1,
        DeleteGuest = 2,
        CheckInOut = 4,
        ReportAccess = 8,
        LocationManagement = 16,
        RoomManagement = 32,
        UserManagement = 64,
        All = 127
    }

    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, NotNull]
        public string Username { get; set; } = string.Empty;

        [NotNull]
        public string Password { get; set; } = string.Empty;

        [NotNull]
        public UserRole Role { get; set; } = UserRole.Normal;

        public int Permissions { get; set; } = (int)Permission.None;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Helper methods
        public bool HasPermission(Permission permission)
        {
            if (Role == UserRole.Admin)
                return true;
            
            return (Permissions & (int)permission) != 0;
        }

        public void GrantPermission(Permission permission)
        {
            Permissions |= (int)permission;
        }

        public void RevokePermission(Permission permission)
        {
            Permissions &= ~(int)permission;
        }

        public string GetPermissionDisplayText()
        {
            if (Role == UserRole.Admin)
                return "All Permissions";
            
            var permissions = new List<string>();
            if (HasPermission(Permission.AddGuest)) permissions.Add("Add Guest");
            if (HasPermission(Permission.DeleteGuest)) permissions.Add("Delete Guest");
            if (HasPermission(Permission.CheckInOut)) permissions.Add("Check In/Out");
            if (HasPermission(Permission.ReportAccess)) permissions.Add("Reports");
            if (HasPermission(Permission.LocationManagement)) permissions.Add("Locations");
            if (HasPermission(Permission.RoomManagement)) permissions.Add("Rooms");
            if (HasPermission(Permission.UserManagement)) permissions.Add("Users");
            
            return permissions.Count > 0 ? string.Join(", ", permissions) : "No Permissions";
        }
    }
}
