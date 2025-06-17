using Jindal.Models;
using Jindal.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Jindal.Views
{
    public partial class AddCheckInOutPage : ContentPage
    {
        public AddCheckInOutPage()
        {
            InitializeComponent();
            LoadAvailableRooms(); // Load rooms when the page is initialized
        }

        /// <summary>
        /// Loads available rooms from the database and binds to the RoomPicker.
        /// </summary>
        private async void LoadAvailableRooms()
        {
            try
            {
                await DatabaseService.Init(); // Ensure database is initialized
                var availableRooms = await DatabaseService.GetAvailableRooms();

                RoomPicker.ItemsSource = availableRooms;
                RoomPicker.ItemDisplayBinding = new Binding("RoomNumber");

                // Debug: Log available rooms
                foreach (var room in availableRooms)
                {
                    System.Diagnostics.Debug.WriteLine($"Room: {room.RoomNumber}, Available: {room.IsAvailable}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading rooms: {ex.Message}");
                await DisplayAlert("Error", "Failed to load available rooms.", "OK");
            }
        }

        /// <summary>
        /// Handles the check-in button click. Validates and saves guest info to the database.
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
                    RoomNumber = selectedRoom?.RoomNumber.ToString() ?? "",

                    IdType = IdTypePicker.SelectedItem?.ToString(),
                    GuestName = GuestNameEntry.Text,
                    GuestIdNumber = IdNumberEntry.Text,
                    CompanyName = CompanyEntry.Text,
                    Mobile = MobileEntry.Text,
                    Address = AddressEntry.Text,
                    Nationality = NationalityEntry.Text,
                    CheckInDate = CheckInDatePicker.Date,
                    CheckInTime = CheckInTimePicker.Time,
                    CheckOutDate = CheckOutDatePicker.Date,
                    CheckOutTime = CheckOutTimePicker.Time,
                    Department = DepartmentEntry.Text,
                    Purpose = PurposeEntry.Text,
                    MailReceivedDate = MailReceivedDatePicker.Date
                };

                await DatabaseService.AddCheckInOut(newEntry);
                await DisplayAlert("Success", "Guest checked in successfully.", "OK");

                await Navigation.PopAsync(); // Navigate back to list or dashboard
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Check-in failed: {ex.Message}");
                await DisplayAlert("Error", "Check-in failed. Please try again.", "OK");
            }
        }

        /// <summary>
        /// Navigates to another page to add a second guest to the same room.
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
                await Shell.Current.GoToAsync($"{nameof(AddGuestToSameRoomPage)}?roomNumber={selectedRoom.RoomNumber}");


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Error", "Unable to navigate to guest form.", "OK");
            }
        }
    }
}
