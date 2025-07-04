using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jindal.Views
{
    public partial class AddEditRoomPage : ContentPage
    {
        private Room _room;
        private List<Jindal.Models.Location> _locations;

        public AddEditRoomPage(Room room = null)
        {
            InitializeComponent();
            _room = room;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Load locations
            _locations = await DatabaseService.GetLocations();

            // Bind picker to location names
            LocationPicker.ItemsSource = _locations.Select(l => l.Name).ToList();

            // Pre-fill if editing
            if (_room != null)
            {
                RoomNumberEntry.Text = _room.RoomNumber.ToString();
                RemarkEntry.Text = _room.Remark;

                // Find the location name for the selected LocationId
                var selectedLocation = _locations.FirstOrDefault(l => l.Id == _room.LocationId);
                if (selectedLocation != null)
                {
                    LocationPicker.SelectedItem = selectedLocation.Name;
                }
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RoomNumberEntry.Text) || LocationPicker.SelectedItem == null)
            {
                await DisplayAlert("Validation Error", "Please fill all required fields.", "OK");
                return;
            }

            if (!int.TryParse(RoomNumberEntry.Text, out int roomNumber))
            {
                await DisplayAlert("Input Error", "Room number must be a valid integer.", "OK");
                return;
            }

            // Get selected location ID
            string selectedLocationName = LocationPicker.SelectedItem.ToString();
            var selectedLocation = _locations.FirstOrDefault(l => l.Name == selectedLocationName);
            int locationId = selectedLocation?.Id ?? 0;

            string availability = _room?.Availability ?? "Available";
            string remark = RemarkEntry.Text?.Trim() ?? "";

            if (_room == null)
            {
                var newRoom = new Room
                {
                    RoomNumber = roomNumber,
                    Availability = availability,
                    LocationId = locationId,
                    Remark = remark
                };

                await DatabaseService.AddRoom(newRoom);
            }
            else
            {
                _room.RoomNumber = roomNumber;
                _room.LocationId = locationId;
                _room.Remark = remark;

                await DatabaseService.UpdateRoom(_room);
            }

            await Navigation.PopAsync();
        }
    }
}
