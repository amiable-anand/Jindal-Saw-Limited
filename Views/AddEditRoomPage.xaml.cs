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
                AvailabilityPicker.SelectedItem = _room.Availability;
                LocationEntry.Text = _room.Location;
                RemarkEntry.Text = _room.Remark;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RoomNumberEntry.Text) ||
                AvailabilityPicker.SelectedItem == null ||
                string.IsNullOrWhiteSpace(LocationEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please fill all required fields.", "OK");
                return;
            }

            int roomNumber = int.Parse(RoomNumberEntry.Text);
            var availability = AvailabilityPicker.SelectedItem.ToString();

            if (_room == null)
            {
                await DatabaseService.AddRoom(new Room
                {
                    RoomNumber = roomNumber,
                    Availability = availability,
                    Location = LocationEntry.Text,
                    Remark = RemarkEntry.Text
                });
            }
            else
            {
                _room.RoomNumber = roomNumber;
                _room.Availability = availability;
                _room.Location = LocationEntry.Text;
                _room.Remark = RemarkEntry.Text;

                await DatabaseService.UpdateRoom(_room);
            }

            await Navigation.PopAsync();
        }
    }
}
