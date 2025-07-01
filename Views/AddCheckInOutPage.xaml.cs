using Jindal.Models;
using Jindal.Services;
using System;
using Microsoft.Maui.Controls;

namespace Jindal.Views
{
    public partial class AddCheckInOutPage : ContentPage
    {
        public AddCheckInOutPage()
        {
            InitializeComponent();

            try
            {
                LoadAvailableRooms();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? Constructor error: {ex.Message}");
            }
        }

        private async void LoadAvailableRooms()
        {
            try
            {
                await DatabaseService.Init();
                var availableRooms = await DatabaseService.GetAvailableRooms();

                RoomPicker.ItemsSource = availableRooms;
                RoomPicker.ItemDisplayBinding = new Binding("RoomNumber");

                foreach (var room in availableRooms)
                {
                    System.Diagnostics.Debug.WriteLine($"Room: {room.RoomNumber}, Available: {room.Availability}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading rooms: {ex.Message}");
                await DisplayAlert("Error", "Failed to load available rooms.", "OK");
            }
        }

        private async void OnCheckInClicked(object sender, EventArgs e)
        {
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
                    GuestName = GuestNameEntry.Text,
                    GuestIdNumber = IdNumberEntry.Text,
                    CompanyName = CompanyEntry.Text,
                    Mobile = MobileEntry.Text,
                    Address = AddressEntry.Text,
                    Nationality = NationalityEntry.Text,
                    Department = DepartmentEntry.Text,
                    Purpose = PurposeEntry.Text,
                    CheckInDate = CheckInDatePicker.Date,
                    CheckInTime = CheckInTimePicker.Time,
                    MailReceivedDate = MailReceivedDatePicker.Date
                };

                await DatabaseService.AddCheckInOut(newEntry);

                if (selectedRoom != null)
                {
                    selectedRoom.Availability = "Booked";
                    await DatabaseService.UpdateRoom(selectedRoom);
                }

                await DisplayAlert("Success", "Guest checked in successfully.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Check-in failed: {ex.Message}");
                await DisplayAlert("Error", "Check-in failed. Please try again.", "OK");
            }
        }

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
