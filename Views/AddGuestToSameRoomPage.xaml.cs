using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace Jindal.Views
{
    // Receives parameters from Shell navigation
    [QueryProperty(nameof(RoomNumber), "roomNumber")]
    [QueryProperty(nameof(GuestId), "guestId")]
    [QueryProperty(nameof(SourcePage), "sourcePage")]
    public partial class AddGuestToSameRoomPage : ContentPage
    {
        // Properties for navigation and room identification
        public string RoomNumber { get; set; } = string.Empty;
        public int GuestId { get; set; } // Used if returning to EditGuestPage
        public string SourcePage { get; set; } = string.Empty;

        public AddGuestToSameRoomPage()
        {
            InitializeComponent();
            
            // Set default date and time values
            CheckInDatePicker.Date = DateTime.Today;
            CheckInTimePicker.Time = DateTime.Now.TimeOfDay;
            MailReceivedDatePicker.Date = DateTime.Today;
        }

        /// <summary>
        /// Triggered when page navigation completes.
        /// Sets the Room Number label dynamically.
        /// </summary>
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            try
            {
                if (!string.IsNullOrEmpty(RoomNumber))
                {
                    RoomNumberLabel.Text = $"Room: {RoomNumber}";
                    Debug.WriteLine($"AddGuestToSameRoomPage: Room Number set to {RoomNumber}");
                }
                else
                {
                    RoomNumberLabel.Text = "Room: Not specified";
                    Debug.WriteLine("AddGuestToSameRoomPage: Room Number is empty");
                }

                Debug.WriteLine($"AddGuestToSameRoomPage: SourcePage = {SourcePage}, GuestId = {GuestId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnNavigatedTo error: {ex.Message}");
                RoomNumberLabel.Text = "Room: Error loading";
            }
        }

        /// <summary>
        /// Saves a new guest entry for the existing room.
        /// </summary>
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(GuestNameEntry.Text) ||
                string.IsNullOrWhiteSpace(GuestIdEntry.Text) ||
                IdTypePicker.SelectedItem == null)
            {
                await DisplayAlert("Validation Error", "Please fill all required fields (Name, ID Type, ID Number).", "OK");
                return;
            }

            try
            {
                // Additional validation for RoomNumber
                if (string.IsNullOrWhiteSpace(RoomNumber))
                {
                    await DisplayAlert("Error", "Room number is missing. Please go back and select a room.", "OK");
                    return;
                }

                var guest = new CheckInOut
                {
                    RoomNumber = RoomNumber,
                    GuestName = GuestNameEntry.Text?.Trim() ?? string.Empty,
                    GuestIdNumber = GuestIdEntry.Text?.Trim() ?? string.Empty,
                    IdType = IdTypePicker.SelectedItem?.ToString() ?? string.Empty,
                    CompanyName = CompanyEntry.Text?.Trim() ?? string.Empty,
                    Nationality = NationalityEntry.Text?.Trim() ?? string.Empty,
                    Address = AddressEntry.Text?.Trim() ?? string.Empty,
                    Mobile = MobileEntry.Text?.Trim() ?? string.Empty,
                    CheckInDate = CheckInDatePicker.Date,
                    CheckInTime = CheckInTimePicker.Time,
                    Department = DepartmentEntry.Text?.Trim() ?? string.Empty,
                    Purpose = PurposeEntry.Text?.Trim() ?? string.Empty,
                    MailReceivedDate = MailReceivedDatePicker.Date
                };

                await DatabaseService.AddCheckInOut(guest);

                await DisplayAlert("Success", "Guest added successfully!", "OK");

                // Navigate back to appropriate page
                NavigateBack();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add guest error: {ex.Message}");
                await DisplayAlert("Error", "Failed to add guest. Please try again.", "OK");
            }
        }

        /// <summary>
        /// Overrides physical back button to ensure correct navigation.
        /// </summary>
        protected override bool OnBackButtonPressed()
        {
            NavigateBack();
            return true; // Block default back behavior
        }

        /// <summary>
        /// Handles navigation back to appropriate source page (EditGuestPage or previous).
        /// </summary>
        private async void NavigateBack()
        {
            try
            {
                if (!string.IsNullOrEmpty(SourcePage) && SourcePage == nameof(EditGuestPage) && GuestId > 0)
                {
                    await Shell.Current.GoToAsync($"{nameof(EditGuestPage)}?guestId={GuestId}");
                }
                else
                {
                    await Shell.Current.GoToAsync(".."); // Go back in navigation stack
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation back error: {ex.Message}");
                // Fallback: try to pop the current page
                await Navigation.PopAsync();
            }
        }
    }
}
