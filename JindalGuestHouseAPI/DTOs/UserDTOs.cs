using System.ComponentModel.DataAnnotations;
using JindalGuestHouseAPI.Models;

namespace JindalGuestHouseAPI.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]{3,50}$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Full name must be between 1 and 200 characters")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email must be 255 characters or less")]
        public string? Email { get; set; }

        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid user role")]
        public UserRole Role { get; set; } = UserRole.Normal;

        [Range(0, 127, ErrorMessage = "Permissions must be between 0 and 127")]
        public int Permissions { get; set; } = 0;
    }

    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]{3,50}$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Full name must be between 1 and 200 characters")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email must be 255 characters or less")]
        public string? Email { get; set; }

        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid user role")]
        public UserRole Role { get; set; } = UserRole.Normal;

        [Range(0, 127, ErrorMessage = "Permissions must be between 0 and 127")]
        public int Permissions { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username must be 50 characters or less")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, ErrorMessage = "Password must be 128 characters or less")]
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 128 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = new();
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int Permissions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string PermissionText { get; set; } = string.Empty;
    }
}
