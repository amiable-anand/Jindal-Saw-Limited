using Jindal.Models;
using Jindal.Services;

namespace Jindal.Views
{
    public partial class AddEditRoomPage : ContentPage
    {
        private Room _room;

        public AddEditRoomPage(Room room = null)
        {
            InitializeComponent();
            _room = room;

            if (_room != null)
            {
                RoomNumberEntry.Text = _room.RoomNumber.ToString();
                LocationEntry.Text = _room.Location;
                RemarkEntry.Text = _room.Remark;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RoomNumberEntry.Text) ||
                string.IsNullOrWhiteSpace(LocationEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please fill all required fields.", "OK");
                return;
            }

            if (!int.TryParse(RoomNumberEntry.Text, out int roomNumber))
            {
                await DisplayAlert("Input Error", "Room number must be a valid integer.", "OK");
                return;
            }

            string availability = _room?.Availability ?? "Available";  // Keep previous or default
            string location = LocationEntry.Text.Trim();
            string remark = RemarkEntry.Text?.Trim() ?? string.Empty;

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
