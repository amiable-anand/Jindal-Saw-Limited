using Jindal.Models;
using Jindal.Services;

namespace Jindal.Views
{
    public partial class AddEditRoomPage : ContentPage
    {
        private Room _room; // Holds the room being edited (null if adding a new one)

        public AddEditRoomPage(Room room = null)
        {
            InitializeComponent();
            _room = room;

            // If editing an existing room, populate the fields
            if (_room != null)
            {
                RoomNumberEntry.Text = _room.RoomNumber.ToString();
                LocationEntry.Text = _room.Location;
                RemarkEntry.Text = _room.Remark;
            }
        }

        /// <summary>
        /// Handles Save button click. Validates input and either adds or updates the room in the database.
        /// </summary>
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(RoomNumberEntry.Text) ||
                string.IsNullOrWhiteSpace(LocationEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please fill all required fields.", "OK");
                return;
            }

            // Validate Room Number input
            if (!int.TryParse(RoomNumberEntry.Text, out int roomNumber))
            {
                await DisplayAlert("Input Error", "Room number must be a valid integer.", "OK");
                return;
            }

            // Set default or retain existing availability
            string availability = _room?.Availability ?? "Available";
            string location = LocationEntry.Text.Trim();
            string remark = RemarkEntry.Text?.Trim() ?? string.Empty;

            // If adding a new room
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
            else // If editing existing room
            {
                _room.RoomNumber = roomNumber;
                _room.Location = location;
                _room.Remark = remark;

                await DatabaseService.UpdateRoom(_room);
            }

            // Navigate back after save
            await Navigation.PopAsync();
        }
    }
}
