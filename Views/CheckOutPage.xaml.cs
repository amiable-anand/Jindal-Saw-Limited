using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jindal.Views
{
    [QueryProperty(nameof(GuestId), "guestId")]
    public partial class CheckOutPage : ContentPage
    {
        private CheckInOut? guest;
        private string guestId = string.Empty;

        // Bound from query via Shell navigation
        public string GuestId
        {
            get => guestId;
            set
            {
                guestId = value;
                _ = LoadGuestDetailsAsync(value); // Fire-and-forget safely
            }
        }

        public CheckOutPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads the guest details from the database using the passed GuestId.
        /// Sets default check-out date/time to now.
        /// </summary>
        private async Task LoadGuestDetailsAsync(string id)
        {
            try
            {
                await DatabaseService.Init();

                if (int.TryParse(id, out int guestIdInt))
                {
                    guest = await DatabaseService.GetCheckInOutById(guestIdInt);

                    if (guest != null)
                    {
                        GuestNameLabel.Text = guest.GuestName;
                        RoomNumberLabel.Text = $"Room: {guest.RoomNumber}";
                        CheckOutDatePicker.Date = DateTime.Now.Date;
                        CheckOutTimePicker.Time = DateTime.Now.TimeOfDay;
                    }
                    else
                    {
                        await DisplayAlert("Error", "Guest not found.", "OK");
                        await NavigationService.NavigateToCheckInOut();
                    }
                }
                else
                {
                    await DisplayAlert("Invalid ID", "Guest ID format is invalid.", "OK");
                    await NavigationService.NavigateToCheckInOut();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load guest details: {ex.Message}");
                await DisplayAlert("Error", $"Failed to load guest details: {ex.Message}", "OK");
                await NavigationService.NavigateToCheckInOut();
            }
        }

        /// <summary>
        /// Handles the Confirm Check-Out button click.
        /// Updates guest check-out info and room availability if needed.
        /// </summary>
        private async void OnConfirmCheckOutClicked(object sender, EventArgs e)
        {
            if (guest == null)
            {
                await DisplayAlert("Error", "Guest information not loaded.", "OK");
                return;
            }

            // Show confirmation dialog
            bool confirm = await DisplayAlert("Confirm Check-Out", 
                $"Are you sure you want to check out {guest.GuestName} from Room {guest.RoomNumber}?", 
                "Yes", "No");
                
            if (!confirm)
                return;

            try
            {
                // Show loading indicator
                var button = sender as Button;
                if (button != null)
                {
                    button.Text = "Processing...";
                    button.IsEnabled = false;
                }

                // Validate checkout date/time
                if (CheckOutDatePicker.Date < guest.CheckInDate.Date)
                {
                    await DisplayAlert("Invalid Date", "Check-out date cannot be before check-in date.", "OK");
                    return;
                }

                try
                {
                    // Update checkout info
                    guest.CheckOutDate = CheckOutDatePicker.Date;
                    guest.CheckOutTime = CheckOutTimePicker.Time;

                    await DatabaseService.UpdateCheckInOut(guest);

                    // Update room availability status
                    await DatabaseService.UpdateRoomAvailabilityStatus();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to check-out guest: {ex.Message}");
                    await DisplayAlert("Error", "Failed to check-out guest.", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Guest {guest.GuestName} checked out from room {guest.RoomNumber}");
                await DisplayAlert("Success", "Guest checked out successfully.", "OK");
                await NavigationService.NavigateToCheckInOut();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Check-out failed: {ex.Message}");
                await DisplayAlert("Error", "Check-out failed.", "OK");
            }
            finally
            {
                // Restore button state
                var button = sender as Button;
                if (button != null)
                {
                    button.Text = "✓ Confirm Check-Out";
                    button.IsEnabled = true;
                }
            }
        }
    }
}
