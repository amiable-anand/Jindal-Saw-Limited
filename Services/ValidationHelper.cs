using System;
using System.Text.RegularExpressions;
using Jindal.Models;

namespace Jindal.Services
{
    /// <summary>
    /// Provides comprehensive validation methods for the application
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates guest check-in data
        /// </summary>
        public static ValidationResult ValidateGuestData(CheckInOut guest)
        {
            var result = new ValidationResult();

            if (guest == null)
            {
                result.AddError("Guest data cannot be null");
                return result;
            }

            // Required fields validation
            if (string.IsNullOrWhiteSpace(guest.GuestName))
                result.AddError("Guest name is required");

            if (string.IsNullOrWhiteSpace(guest.GuestIdNumber))
                result.AddError("Guest ID number is required");

            if (string.IsNullOrWhiteSpace(guest.RoomNumber))
                result.AddError("Room number is required");

            if (string.IsNullOrWhiteSpace(guest.IdType))
                result.AddError("ID type is required");

            // Length validations
            if (!string.IsNullOrWhiteSpace(guest.GuestName) && guest.GuestName.Length > 100)
                result.AddError("Guest name must be 100 characters or less");

            if (!string.IsNullOrWhiteSpace(guest.GuestIdNumber) && guest.GuestIdNumber.Length > 50)
                result.AddError("Guest ID number must be 50 characters or less");

            if (!string.IsNullOrWhiteSpace(guest.Mobile) && guest.Mobile.Length > 20)
                result.AddError("Mobile number must be 20 characters or less");

            // Phone number validation
            if (!string.IsNullOrWhiteSpace(guest.Mobile) && !IsValidPhoneNumber(guest.Mobile))
                result.AddError("Invalid mobile number format");

            // Email validation
            if (!string.IsNullOrWhiteSpace(guest.CompanyName) && guest.CompanyName.Contains("@") && !IsValidEmail(guest.CompanyName))
                result.AddError("Invalid email format in company name");

            // Date validation
            if (guest.CheckInDate == default)
                result.AddError("Check-in date is required");

            if (guest.CheckInDate > DateTime.Now.AddDays(1))
                result.AddError("Check-in date cannot be more than 1 day in the future");

            if (guest.CheckInDate < DateTime.Now.AddDays(-30))
                result.AddError("Check-in date cannot be more than 30 days in the past");

            return result;
        }

        /// <summary>
        /// Validates user data
        /// </summary>
        public static ValidationResult ValidateUserData(User user, bool isNew = false)
        {
            var result = new ValidationResult();

            if (user == null)
            {
                result.AddError("User data cannot be null");
                return result;
            }

            // Required fields validation
            if (string.IsNullOrWhiteSpace(user.Username))
                result.AddError("Username is required");

            if (string.IsNullOrWhiteSpace(user.FullName))
                result.AddError("Full name is required");

            if (isNew && string.IsNullOrWhiteSpace(user.Password))
                result.AddError("Password is required for new users");

            // Length validations
            if (!string.IsNullOrWhiteSpace(user.Username) && user.Username.Length > 50)
                result.AddError("Username must be 50 characters or less");

            if (!string.IsNullOrWhiteSpace(user.FullName) && user.FullName.Length > 100)
                result.AddError("Full name must be 100 characters or less");

            // Username format validation
            if (!string.IsNullOrWhiteSpace(user.Username) && !IsValidUsername(user.Username))
                result.AddError("Username can only contain letters, numbers, and underscores");

            // Email validation
            if (!string.IsNullOrWhiteSpace(user.Email) && !IsValidEmail(user.Email))
                result.AddError("Invalid email format");

            // Password validation for new users
            if (isNew && !string.IsNullOrWhiteSpace(user.Password))
            {
                var passwordResult = ValidatePassword(user.Password);
                if (!passwordResult.IsValid)
                    result.AddErrors(passwordResult.Errors);
            }

            return result;
        }

        /// <summary>
        /// Validates location data
        /// </summary>
        public static ValidationResult ValidateLocationData(Jindal.Models.Location location)
        {
            var result = new ValidationResult();

            if (location == null)
            {
                result.AddError("Location data cannot be null");
                return result;
            }

            // Required fields validation
            if (string.IsNullOrWhiteSpace(location.Name))
                result.AddError("Location name is required");

            if (string.IsNullOrWhiteSpace(location.LocationCode))
                result.AddError("Location code is required");

            // Length validations
            if (!string.IsNullOrWhiteSpace(location.Name) && location.Name.Length > 100)
                result.AddError("Location name must be 100 characters or less");

            if (!string.IsNullOrWhiteSpace(location.LocationCode) && location.LocationCode.Length > 10)
                result.AddError("Location code must be 10 characters or less");

            if (!string.IsNullOrWhiteSpace(location.Address) && location.Address.Length > 500)
                result.AddError("Address must be 500 characters or less");

            // Location code format validation
            if (!string.IsNullOrWhiteSpace(location.LocationCode) && !IsValidLocationCode(location.LocationCode))
                result.AddError("Location code can only contain letters, numbers, and hyphens");

            return result;
        }

        /// <summary>
        /// Validates room data
        /// </summary>
        public static ValidationResult ValidateRoomData(Room room)
        {
            var result = new ValidationResult();

            if (room == null)
            {
                result.AddError("Room data cannot be null");
                return result;
            }

            // Required fields validation
            if (room.RoomNumber <= 0)
                result.AddError("Room number must be greater than 0");

            if (room.LocationId <= 0)
                result.AddError("Location is required");

            // Range validations
            if (room.RoomNumber > 9999)
                result.AddError("Room number cannot exceed 9999");

            // Length validations
            if (!string.IsNullOrWhiteSpace(room.Remark) && room.Remark.Length > 500)
                result.AddError("Remark must be 500 characters or less");

            return result;
        }

        /// <summary>
        /// Validates password strength
        /// </summary>
        public static ValidationResult ValidatePassword(string password)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(password))
            {
                result.AddError("Password is required");
                return result;
            }

            if (password.Length < 8)
                result.AddError("Password must be at least 8 characters long");

            if (password.Length > 128)
                result.AddError("Password cannot exceed 128 characters");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                result.AddError("Password must contain at least one uppercase letter");

            if (!Regex.IsMatch(password, @"[a-z]"))
                result.AddError("Password must contain at least one lowercase letter");

            if (!Regex.IsMatch(password, @"[0-9]"))
                result.AddError("Password must contain at least one number");

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?\"":{}|<>]"))
                result.AddError("Password must contain at least one special character");

            return result;
        }

        /// <summary>
        /// Validates phone number format
        /// </summary>
        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Allow international format, digits, spaces, parentheses, hyphens, and plus sign
            var phonePattern = @"^[\+]?[0-9\s\-\(\)]{7,20}$";
            return Regex.IsMatch(phoneNumber, phonePattern);
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, emailPattern);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates username format
        /// </summary>
        private static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var usernamePattern = @"^[a-zA-Z0-9_]{3,50}$";
            return Regex.IsMatch(username, usernamePattern);
        }

        /// <summary>
        /// Validates location code format
        /// </summary>
        private static bool IsValidLocationCode(string locationCode)
        {
            if (string.IsNullOrWhiteSpace(locationCode))
                return false;

            var locationCodePattern = @"^[a-zA-Z0-9\-]{2,10}$";
            return Regex.IsMatch(locationCode, locationCodePattern);
        }
    }

    /// <summary>
    /// Represents validation results
    /// </summary>
    public class ValidationResult
    {
        private readonly List<string> _errors = new();

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                _errors.Add(error);
        }

        public void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
                AddError(error);
        }

        public string GetErrorMessage()
        {
            return string.Join("\n", _errors);
        }
    }
}
