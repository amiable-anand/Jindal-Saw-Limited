using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jindal.Views
{
    public partial class AddCheckInOutPage : ContentPage
    {
        public AddCheckInOutPage()
        {
            InitializeComponent();
            
            // Set default date and time values
            CheckInDatePicker.Date = DateTime.Today;
            CheckInTimePicker.Time = DateTime.Now.TimeOfDay;
            MailReceivedDatePicker.Date = DateTime.Today;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Load available rooms when the page appears
            try
            {
                await LoadAvailableRoomsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnAppearing Error: {ex.Message}");
                await DisplayAlert("Error", "Failed to load room data.", "OK");
            }
        }

        /// <summary>
        /// Loads rooms that are completely available from the database.
        /// </summary>
        private async Task LoadAvailableRoomsAsync()
        {
            try
            {
                await DatabaseService.Init();
                var availableRooms = await DatabaseService.GetCompletelyAvailableRooms();

                if (availableRooms == null || !availableRooms.Any())
                {
                    await DisplayAlert("No Rooms Available", 
                        "No rooms are currently available. Please add rooms in the Rooms section first.", 
                        "OK");
                    RoomPicker.ItemsSource = new List<Room>();
                    return;
                }

                RoomPicker.ItemsSource = availableRooms;
                RoomPicker.ItemDisplayBinding = new Binding("RoomNumber");

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Loaded {availableRooms.Count} available rooms:");
                foreach (var room in availableRooms)
                {
                    System.Diagnostics.Debug.WriteLine($"Room: {room.RoomNumber}, Available: {room.Availability}, Location: {room.LocationName}");
                }
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading rooms: {ex.Message}");
                await DisplayAlert("Error", $"Failed to load available rooms: {ex.Message}", "OK");
                RoomPicker.ItemsSource = new List<Room>();
            }
        }

        /// <summary>
        /// Handles guest check-in process, including validation and DB save.
        /// </summary>
        private async void OnCheckInClicked(object sender, EventArgs e)
        {
            // Enhanced validation
            if (RoomPicker.SelectedItem == null)
            {
                await DisplayAlert("Missing Information", "Please select a room first.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(GuestNameEntry.Text))
            {
                await DisplayAlert("Missing Information", "Please enter the guest name.", "OK");
                return;
            }

            if (IdTypePicker.SelectedItem == null)
            {
                await DisplayAlert("Missing Information", "Please select an ID type.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(IdNumberEntry.Text))
            {
                await DisplayAlert("Missing Information", "Please enter the ID number.", "OK");
                return;
            }

            try
            {
                var selectedRoom = RoomPicker.SelectedItem as Room;
                if (selectedRoom == null)
                {
                    await DisplayAlert("Error", "Invalid room selection.", "OK");
                    return;
                }

                var newEntry = new CheckInOut
                {
                    RoomNumber = selectedRoom.RoomNumber.ToString(),
                    IdType = IdTypePicker.SelectedItem.ToString() ?? string.Empty,
                    GuestName = GuestNameEntry.Text?.Trim() ?? string.Empty,
                    GuestIdNumber = IdNumberEntry.Text?.Trim() ?? string.Empty,
                    CompanyName = CompanyEntry.Text?.Trim() ?? string.Empty,
                    Mobile = MobileEntry.Text?.Trim() ?? string.Empty,
                    Address = AddressEntry.Text?.Trim() ?? string.Empty,
                    Nationality = NationalityEntry.Text?.Trim() ?? string.Empty,
                    Department = DepartmentEntry.Text?.Trim() ?? string.Empty,
                    Purpose = PurposeEntry.Text?.Trim() ?? string.Empty,
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
