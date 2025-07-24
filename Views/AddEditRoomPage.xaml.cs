using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jindal.Views
{
    public partial class AddEditRoomPage : ContentPage
    {
        private Room? editingRoom; // Null if adding new
        private List<Jindal.Models.Location> allLocations = new();

        public AddEditRoomPage()
        {
            InitializeComponent();
            LoadLocationsAsync();
        }

        public AddEditRoomPage(Room room) : this()
        {
            editingRoom = room;
            LoadRoomData();
        }

        private async void LoadLocationsAsync()
        {
            try
            {
                await DatabaseService.Init();
                allLocations = await DatabaseService.GetLocations();

                LocationPicker.ItemsSource = allLocations;

                if (editingRoom != null)
                {
                    var selectedLocation = allLocations.FirstOrDefault(l => l.Id == editingRoom.LocationId);
                    if (selectedLocation != null)
                        LocationPicker.SelectedItem = selectedLocation;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load locations: {ex.Message}", "OK");
            }
        }

        private void LoadRoomData()
        {
            if (editingRoom == null)
                return;

            RoomNumberEntry.Text = editingRoom.RoomNumber.ToString();
            RemarkEntry.Text = editingRoom.Remark;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (!int.TryParse(RoomNumberEntry.Text?.Trim(), out int roomNumber))
            {
                await DisplayAlert("Error", "Please enter a valid Room Number.", "OK");
                return;
            }

            if (LocationPicker.SelectedItem is not Jindal.Models.Location selectedLocation)
            {
                await DisplayAlert("Error", "Please select a location.", "OK");
                return;
            }

            var remark = RemarkEntry.Text?.Trim() ?? "";

            try
            {
                // Create room for validation
                var roomToValidate = editingRoom ?? new Room();
                roomToValidate.RoomNumber = roomNumber;
                roomToValidate.LocationId = selectedLocation.Id;
                roomToValidate.Remark = remark;

                // Validate room data
                var validationResult = ValidationHelper.ValidateRoomData(roomToValidate);
                if (!validationResult.IsValid)
                {
                    await DisplayAlert("Validation Error", validationResult.GetErrorMessage(), "OK");
                    return;
                }

                // Save room
                if (editingRoom == null)
                {
                    var newRoom = new Room
                    {
                        RoomNumber = roomNumber,
                        LocationId = selectedLocation.Id,
                        Remark = remark,
                        Availability = "Available"
                    };
                    await DatabaseService.AddRoom(newRoom);
                    System.Diagnostics.Debug.WriteLine($"New room added: {newRoom.RoomNumber}");
                }
                else
                {
                    editingRoom.RoomNumber = roomNumber;
                    editingRoom.LocationId = selectedLocation.Id;
                    editingRoom.Remark = remark;
                    await DatabaseService.UpdateRoom(editingRoom);
                    System.Diagnostics.Debug.WriteLine($"Room updated: {editingRoom.RoomNumber} (ID={editingRoom.Id})");
                }

                await DisplayAlert("Success", "Room saved successfully.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save room: {ex.Message}");
                await DisplayAlert("Error", $"Failed to save room: {ex.Message}", "OK");
            }
        }
    }
}
