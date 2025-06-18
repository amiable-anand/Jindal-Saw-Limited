using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;

namespace Jindal.Views
{
    public partial class AddEditRoomPage : ContentPage
    {
        private Room _room;

        public AddEditRoomPage(Room room = null)
        {
            InitializeComponent();
            _room = room;

            // If editing, populate fields
            if (_room != null)
            {
                RoomNumberEntry.Text = _room.RoomNumber.ToString();

                // Set selected item for LocationPicker
                if (!string.IsNullOrWhiteSpace(_room.Location))
                    LocationPicker.SelectedItem = _room.Location;

                // Set text for RemarkEntry instead of Picker
                if (!string.IsNullOrWhiteSpace(_room.Remark))
                    RemarkEntry.Text = _room.Remark;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RoomNumberEntry.Text) ||
                LocationPicker.SelectedItem == null)
            {
                await DisplayAlert("Validation Error", "Please fill all required fields.", "OK");
                return;
            }

            if (!int.TryParse(RoomNumberEntry.Text, out int roomNumber))
            {
                await DisplayAlert("Input Error", "Room number must be a valid integer.", "OK");
                return;
            }

            string availability = _room?.Availability ?? "Available";
            string location = LocationPicker.SelectedItem.ToString();
            string remark = RemarkEntry.Text?.Trim() ?? "";

            if (_room == null)
            {
                var newRoom = new Room
                {
                    RoomNumber = roomNumber,
                    Availability = availability,
                    Location = location,
                    Remark = remark
                };

                await DatabaseService.AddRoom(newRoom);
            }
            else
            {
                _room.RoomNumber = roomNumber;
                _room.Location = location;
                _room.Remark = remark;

                await DatabaseService.UpdateRoom(_room);
            }

            await Navigation.PopAsync();
        }
    }
}
