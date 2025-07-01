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

        public string GuestId
        {
            get => guestId;
            set
            {
                guestId = value;
                _ = LoadGuestDetailsAsync(value); // Fire and forget
            }
        }

        public CheckOutPage()
        {
            InitializeComponent();
        }

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
                        RoomNumberLabel.Text = guest.RoomNumber;
                        CheckOutDatePicker.Date = DateTime.Now.Date;
                        CheckOutTimePicker.Time = DateTime.Now.TimeOfDay;
                    }
                    else
                    {
                        await DisplayAlert("Error", "Guest not found.", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load guest details: {ex.Message}", "OK");
            }
        }

        private async void OnConfirmCheckOutClicked(object sender, EventArgs e)
        {
            if (guest == null)
                return;

            try
            {
                // ? Update the guest's check-out info
                guest.CheckOutDate = CheckOutDatePicker.Date;
                guest.CheckOutTime = CheckOutTimePicker.Time;

                await DatabaseService.UpdateCheckInOut(guest);

                // ? Check if all guests in this room are checked out
                var guestsInRoom = await DatabaseService.GetCheckInOutsByRoomNumber(guest.RoomNumber);
                bool allCheckedOut = guestsInRoom.All(g => g.CheckOutDate != null && g.CheckOutTime != null);

                if (allCheckedOut)
                {
                    var room = (await DatabaseService.GetRooms())
                               .FirstOrDefault(r => r.RoomNumber.ToString() == guest.RoomNumber);

                    if (room != null)
                    {
                        room.Availability = "Available"; // Optional — for UI reflection
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
