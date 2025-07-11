using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jindal.Models;
using Microsoft.Maui.Controls;

namespace Jindal.Services
{
    /// <summary>
    /// Centralized navigation service to eliminate parameter complexity and provide consistent navigation
    /// </summary>
    public static class NavigationService
    {
        // Navigation context to maintain state
        private static NavigationContext _currentContext = new();

        /// <summary>
        /// Navigate to Add Guest page with room context
        /// </summary>
        public static async Task NavigateToAddGuest(int? roomNumber = null, int? associatedGuestId = null)
        {
            try
            {
                _currentContext.SetContext(roomNumber, associatedGuestId);
                await Shell.Current.GoToAsync(nameof(Views.AddCheckInOutPage));
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to navigate to Add Guest page", ex, "NavigationService");
                throw;
            }
        }

        /// <summary>
        /// Navigate to Edit Guest page
        /// </summary>
        public static async Task NavigateToEditGuest(int guestId)
        {
            try
            {
                _currentContext.SetContext(guestId: guestId);
                await Shell.Current.GoToAsync($"{nameof(Views.EditGuestPage)}?guestId={guestId}");
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to navigate to Edit Guest page", ex, "NavigationService");
                throw;
            }
        }

        /// <summary>
        /// Navigate to Add Guest to Same Room page
        /// </summary>
        public static async Task NavigateToAddGuestToSameRoom(int roomNumber, int? associatedGuestId = null)
        {
            try
            {
                _currentContext.SetContext(roomNumber, associatedGuestId);
                await Shell.Current.GoToAsync($"{nameof(Views.AddGuestToSameRoomPage)}?roomNumber={roomNumber}");
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to navigate to Add Guest to Same Room page", ex, "NavigationService");
                throw;
            }
        }

        /// <summary>
        /// Navigate to Check Out page
        /// </summary>
        public static async Task NavigateToCheckOut(int guestId)
        {
            try
            {
                _currentContext.SetContext(guestId: guestId);
                await Shell.Current.GoToAsync($"{nameof(Views.CheckOutPage)}?guestId={guestId}");
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to navigate to Check Out page", ex, "NavigationService");
                throw;
            }
        }

        /// <summary>
        /// Navigate back to Check In/Out page
        /// </summary>
        public static async Task NavigateToCheckInOut()
        {
            try
            {
                _currentContext.Clear();
                await Shell.Current.GoToAsync("//CheckInOutPage");
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to navigate to Check In/Out page", ex, "NavigationService");
                throw;
            }
        }

        /// <summary>
        /// Navigate back to previous page or default page
        /// </summary>
        public static async Task NavigateBack()
        {
            try
            {
                if (_currentContext.HasGuestId)
                {
                    await NavigateToEditGuest(_currentContext.GuestId!.Value);
                }
                else
                {
                    await NavigateToCheckInOut();
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to navigate back", ex, "NavigationService");
                await NavigateToCheckInOut(); // Fallback
            }
        }

        /// <summary>
        /// Get current navigation context
        /// </summary>
        public static NavigationContext GetCurrentContext() => _currentContext;

        /// <summary>
        /// Clear navigation context
        /// </summary>
        public static void ClearContext() => _currentContext.Clear();
    }

    /// <summary>
    /// Navigation context to maintain state between pages
    /// </summary>
    public class NavigationContext
    {
        public int? RoomNumber { get; private set; }
        public int? GuestId { get; private set; }
        public int? AssociatedGuestId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public bool HasRoomNumber => RoomNumber.HasValue && RoomNumber.Value > 0;
        public bool HasGuestId => GuestId.HasValue && GuestId.Value > 0;
        public bool HasAssociatedGuestId => AssociatedGuestId.HasValue && AssociatedGuestId.Value > 0;

        public void SetContext(int? roomNumber = null, int? associatedGuestId = null, int? guestId = null)
        {
            RoomNumber = roomNumber;
            AssociatedGuestId = associatedGuestId;
            GuestId = guestId;
            CreatedAt = DateTime.Now;
        }

        public void Clear()
        {
            RoomNumber = null;
            GuestId = null;
            AssociatedGuestId = null;
            CreatedAt = DateTime.Now;
        }
    }
}
