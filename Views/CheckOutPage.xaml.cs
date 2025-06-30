using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jindal.Views
{
    [QueryProperty("GuestId", "guestId")]
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
                LoadGuestDetails(value);
            }
        }

        public CheckOutPage()
        {
            InitializeComponent();
        }

        private async void LoadGuestDetails(string id)
        {
            await DatabaseService.Init();

            if (int.TryParse(id, out int guestIdInt))
            {
                guest = (await DatabaseService.GetCheckInOuts())
                    .FirstOrDefault(g => g.Id == guestIdInt);

                if (guest != null)
                {
                    GuestNameLabel.Text = guest.GuestName;
                    RoomNumberLabel.Text = guest.RoomNumber;
                    CheckOutDatePicker.Date = DateTime.Now;
                    CheckOutTimePicker.Time = DateTime.Now.TimeOfDay;
                }
            }
        }

        private async void OnConfirmCheckOutClicked(object sender, EventArgs e)
        {
            if (guest == null) return;

            guest.CheckOutDate = CheckOutDatePicker.Date;
            guest.CheckOutTime = CheckOutTimePicker.Time;

            await DatabaseService.UpdateCheckInOut(guest);

            // Check if all guests in this room have checked out
            var allGuests = await DatabaseService.GetCheckInOuts();
            var guestsInRoom = allGuests.Where(g => g.RoomNumber == guest.RoomNumber);
            bool allCheckedOut = guestsInRoom.All(g => g.CheckOutDate != null && g.CheckOutTime != null);

            if (allCheckedOut)
            {
                var rooms = await DatabaseService.GetRooms();
                var room = rooms.FirstOrDefault(r => r.RoomNumber.ToString() == guest.RoomNumber);
                if (room != null)
                {
                    room.Availability = "Available";
                    await DatabaseService.UpdateRoom(room);
                }
            }

            await DisplayAlert("Success", "Guest checked out successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}
