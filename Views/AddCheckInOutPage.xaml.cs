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
                await ProfessionalFeaturesService.ShowError("Failed to load room data.");
            }
        }

        /// <summary>
        /// Loads rooms that are completely available from the database.
        /// </summary>
        private async Task LoadAvailableRoomsAsync()
        {
            try
            {
                var availableRooms = await DatabaseService.GetAvailableRoomsWithLogic();

                if (availableRooms == null || !availableRooms.Any())
                {
                    await ProfessionalFeaturesService.ShowWarning(
                        "No rooms are currently available. Please add rooms in the Rooms section first.");
                    RoomPicker.ItemsSource = new List<Room>();
                    return;
                }

                RoomPicker.ItemsSource = availableRooms;
                RoomPicker.ItemDisplayBinding = new Binding("RoomNumber");

                System.Diagnostics.Debug.WriteLine($"Loaded {availableRooms.Count} available rooms for check-in");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading available rooms: {ex.Message}");
                await DisplayAlert("Error", "Failed to load available rooms. Please try again.", "OK");
                RoomPicker.ItemsSource = new List<Room>();
            }
        }

        /// <summary>
        /// Handles guest check-in process, including validation and DB save.
        /// </summary>
        private async void OnCheckInClicked(object sender, EventArgs e)
        {
            // Use ProfessionalFeaturesService for loading and validation
            await ProfessionalFeaturesService.ExecuteWithLoading(async () =>
            {
                // Enhanced validation
                if (RoomPicker.SelectedItem == null)
                {
                    await ProfessionalFeaturesService.ShowWarning("Please select a room first.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(GuestNameEntry.Text))
                {
                    await ProfessionalFeaturesService.ShowWarning("Please enter the guest name.");
                    return;
                }

                if (IdTypePicker.SelectedItem == null)
                {
                    await ProfessionalFeaturesService.ShowWarning("Please select an ID type.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(IdNumberEntry.Text))
                {
                    await ProfessionalFeaturesService.ShowWarning("Please enter the ID number.");
                    return;
                }

                var selectedRoom = RoomPicker.SelectedItem as Room;
                if (selectedRoom == null)
                {
                    await ProfessionalFeaturesService.ShowError("Invalid room selection.");
                    return;
                }

                var newEntry = new CheckInOut
                {
                    RoomNumber = selectedRoom.RoomNumber,
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

                // Validate guest data using ProfessionalFeaturesService
                var validationResult = ValidationHelper.ValidateGuestData(newEntry);
                if (!validationResult.IsValid)
                {
                    await ProfessionalFeaturesService.ShowValidationErrors(validationResult);
                    return;
                }

                // Add check-in entry and update room status
                await DatabaseService.AddCheckInOut(newEntry);
                
                // Update room status to Booked
                selectedRoom.Availability = "Booked";
                await DatabaseService.UpdateRoom(selectedRoom);

                System.Diagnostics.Debug.WriteLine($"Guest {newEntry.GuestName} checked in to room {newEntry.RoomNumber}");
                await ProfessionalFeaturesService.ShowSuccess("Guest checked in successfully.");
                await NavigationService.NavigateToCheckInOut(); // Go back after success
            }, "Processing check-in...");
        }

    }
}
