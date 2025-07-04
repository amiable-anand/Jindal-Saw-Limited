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
        private CheckInOut guest;
        private string guestId;

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
                        await Shell.Current.GoToAsync("..");
                    }
                }
                else
                {
                    await DisplayAlert("Invalid ID", "Guest ID format is invalid.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load guest details: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Handles the Confirm Check-Out button click.
        /// Updates guest check-out info and room availability if needed.
        /// </summary>
        private async void OnConfirmCheckOutClicked(object sender, EventArgs e)
        {
            if (guest == null)
                return;

            try
            {
                // Update checkout info
                guest.CheckOutDate = CheckOutDatePicker.Date;
                guest.CheckOutTime = CheckOutTimePicker.Time;

                await DatabaseService.UpdateCheckInOut(guest);

                // Check if room can be marked as Available
                var guestsInSameRoom = await DatabaseService.GetCheckInOutsByRoomNumber(guest.RoomNumber);
                bool allGuestsCheckedOut = guestsInSameRoom.All(g =>
                    g.CheckOutDate != null && g.CheckOutTime != null);

                if (allGuestsCheckedOut)
                {
                    var allRooms = await DatabaseService.GetRooms();
                    var room = allRooms.FirstOrDefault(r =>
                        r.RoomNumber.ToString() == guest.RoomNumber);

                    if (room != null)
                    {
                        room.Availability = "Available";
                        await DatabaseService.UpdateRoom(room);
                    }
                }

                await DisplayAlert("Success", "Guest checked out successfully.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Check-out failed: {ex.Message}", "OK");
            }
        }
    }
}
