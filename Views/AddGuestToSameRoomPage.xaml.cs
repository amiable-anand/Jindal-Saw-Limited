using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;

namespace Jindal.Views
{
    // Receives parameters from Shell navigation
    [QueryProperty(nameof(RoomNumber), "roomNumber")]
    [QueryProperty(nameof(GuestId), "guestId")]
    [QueryProperty(nameof(SourcePage), "sourcePage")]
    public partial class AddGuestToSameRoomPage : ContentPage
    {
        // Properties for navigation and room identification
        public string RoomNumber { get; set; }
        public int GuestId { get; set; } // Used if returning to EditGuestPage
        public string SourcePage { get; set; }

        public AddGuestToSameRoomPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Triggered when page navigation completes.
        /// Sets the Room Number label dynamically.
        /// </summary>
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (!string.IsNullOrEmpty(RoomNumber))
                RoomNumberLabel.Text = $"Room: {RoomNumber}";
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
                var guest = new CheckInOut
                {
                    RoomNumber = RoomNumber,
                    GuestName = GuestNameEntry.Text?.Trim(),
                    GuestIdNumber = GuestIdEntry.Text?.Trim(),
                    IdType = IdTypePicker.SelectedItem?.ToString(),
                    CompanyName = CompanyEntry.Text?.Trim(),
                    Nationality = NationalityEntry.Text?.Trim(),
                    Address = AddressEntry.Text?.Trim(),
                    Mobile = MobileEntry.Text?.Trim(),
                    CheckInDate = CheckInDatePicker.Date,
                    CheckInTime = CheckInTimePicker.Time,
                    Department = DepartmentEntry.Text?.Trim(),
                    Purpose = PurposeEntry.Text?.Trim(),
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
            if (SourcePage == nameof(EditGuestPage))
            {
                await Shell.Current.GoToAsync($"{nameof(EditGuestPage)}?guestId={GuestId}");
            }
            else
            {
                await Shell.Current.GoToAsync(".."); // Go back in navigation stack
            }
        }
    }
}
