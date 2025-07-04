using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;

namespace Jindal.Views
{
    public partial class AddCheckInOutPage : ContentPage
    {
        public AddCheckInOutPage()
        {
            InitializeComponent();

            // Load available rooms when the page is initialized
            try
            {
                LoadAvailableRooms();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Constructor Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads rooms that are completely available from the database.
        /// </summary>
        private async void LoadAvailableRooms()
        {
            try
            {
                await DatabaseService.Init();
                var availableRooms = await DatabaseService.GetCompletelyAvailableRooms();

                RoomPicker.ItemsSource = availableRooms;
                RoomPicker.ItemDisplayBinding = new Binding("RoomNumber");

#if DEBUG
                foreach (var room in availableRooms)
                {
                    System.Diagnostics.Debug.WriteLine($"Room: {room.RoomNumber}, Available: {room.Availability}");
                }
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading rooms: {ex.Message}");
                await DisplayAlert("Error", "Failed to load available rooms.", "OK");
            }
        }

        /// <summary>
        /// Handles guest check-in process, including validation and DB save.
        /// </summary>
        private async void OnCheckInClicked(object sender, EventArgs e)
        {
            // Basic validation
            if (RoomPicker.SelectedItem == null || string.IsNullOrWhiteSpace(GuestNameEntry.Text))
            {
                await DisplayAlert("Missing Information", "Please select a room and enter guest name.", "OK");
                return;
            }

            try
            {
                var selectedRoom = RoomPicker.SelectedItem as Room;

                var newEntry = new CheckInOut
                {
                    RoomNumber = selectedRoom?.RoomNumber.ToString(),
                    IdType = IdTypePicker.SelectedItem?.ToString(),
                    GuestName = GuestNameEntry.Text?.Trim(),
                    GuestIdNumber = IdNumberEntry.Text?.Trim(),
                    CompanyName = CompanyEntry.Text?.Trim(),
                    Mobile = MobileEntry.Text?.Trim(),
                    Address = AddressEntry.Text?.Trim(),
                    Nationality = NationalityEntry.Text?.Trim(),
                    Department = DepartmentEntry.Text?.Trim(),
                    Purpose = PurposeEntry.Text?.Trim(),
                    CheckInDate = CheckInDatePicker.Date,
                    CheckInTime = CheckInTimePicker.Time,
                    MailReceivedDate = MailReceivedDatePicker.Date
                };

                await DatabaseService.AddCheckInOut(newEntry);

                // Update room status to Booked
                if (selectedRoom != null)
                {
                    selectedRoom.Availability = "Booked";
                    await DatabaseService.UpdateRoom(selectedRoom);
                }

                await DisplayAlert("Success", "Guest checked in successfully.", "OK");
                await Navigation.PopAsync(); // Go back after success
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Check-in failed: {ex.Message}");
                await DisplayAlert("Error", "Check-in failed. Please try again.", "OK");
            }
        }

        /// <summary>
        /// Navigate to add additional guests to the same room.
        /// </summary>
        private async void OnAddGuestClicked(object sender, EventArgs e)
        {
            if (RoomPicker.SelectedItem == null)
            {
                await DisplayAlert("Select Room", "Please select a room first.", "OK");
                return;
            }

            try
            {
                var selectedRoom = RoomPicker.SelectedItem as Room;
                if (selectedRoom != null)
                {
                    await Shell.Current.GoToAsync($"{nameof(AddGuestToSameRoomPage)}?roomNumber={selectedRoom.RoomNumber}&sourcePage={nameof(AddCheckInOutPage)}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Error", "Unable to navigate to guest form.", "OK");
            }
        }
    }
}
