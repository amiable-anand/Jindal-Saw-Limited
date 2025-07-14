using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Jindal.Views
{
[QueryProperty(nameof(RoomNumber), "roomNumber")]
    public partial class AddGuestToSameRoomPage : ContentPage
    {
        public int RoomNumber { get; set; }

        public AddGuestToSameRoomPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"XAML init error: {ex}");
                throw;
            }

            CheckInDatePicker.Date = DateTime.Today;
            CheckInTimePicker.Time = DateTime.Now.TimeOfDay;
            MailReceivedDatePicker.Date = DateTime.Today;
        }

        /// <summary>
        /// Triggered when page is shown to user. Replaces OnNavigatedTo for reliability with query parameters.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
            if (RoomNumber <= 0)
            {
                ErrorHandlingService.LogError("Invalid room number in navigation context", null, "AddGuestToSameRoomPage");
                await NavigationService.NavigateToCheckInOut(); // Fallback to Check In/Out page
                return;
            }

                if (RoomNumberLabel != null)
                    RoomNumberLabel.Text = $"Room: {RoomNumber}";

                Debug.WriteLine($"[AddGuestToSameRoomPage] RoomNumber: {RoomNumber}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to initialize page: {ex.Message}", "OK");
                Debug.WriteLine($"AddGuestToSameRoomPage.OnAppearing error: {ex}");

                if (RoomNumberLabel != null)
                    RoomNumberLabel.Text = "Room: Error loading";
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GuestNameEntry?.Text))
            {
                await DisplayAlert("Missing Information", "Please enter the guest name.", "OK");
                return;
            }

            if (IdTypePicker?.SelectedItem == null)
            {
                await DisplayAlert("Missing Information", "Please select an ID type.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(GuestIdEntry?.Text))
            {
                await DisplayAlert("Missing Information", "Please enter the ID number.", "OK");
                return;
            }

            try
            {
                if (RoomNumber <= 0)
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
                    CompanyName = CompanyEntry?.Text?.Trim() ?? string.Empty,
                    Nationality = NationalityEntry?.Text?.Trim() ?? string.Empty,
                    Address = AddressEntry?.Text?.Trim() ?? string.Empty,
                    Mobile = MobileEntry?.Text?.Trim() ?? string.Empty,
                    CheckInDate = CheckInDatePicker.Date,
                    CheckInTime = CheckInTimePicker.Time,
                    Department = DepartmentEntry?.Text?.Trim() ?? string.Empty,
                    Purpose = PurposeEntry?.Text?.Trim() ?? string.Empty,
                    MailReceivedDate = MailReceivedDatePicker.Date
                };

                var validationResult = ValidationHelper.ValidateGuestData(guest);
                if (!validationResult.IsValid)
                {
                    await DisplayAlert("Validation Error", validationResult.GetErrorMessage(), "OK");
                    return;
                }

                await ErrorHandlingService.ExecuteWithRetry(async () =>
                {
                    await DatabaseService.AddCheckInOut(guest);
                }, 3, "Add Guest to Same Room");

                ErrorHandlingService.LogInfo($"Guest {guest.GuestName} added to room {guest.RoomNumber}", "AddGuestToSameRoom");
                await DisplayAlert("Success", "Guest added successfully!", "OK");

                // Navigate back using NavigationService which has proper context
                await NavigationService.NavigateBack();
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Add guest to same room failed", ex, "AddGuestToSameRoomPage");
                var userMessage = ErrorHandlingService.GetUserFriendlyErrorMessage(ex);
                await DisplayAlert("Error", userMessage, "OK");
            }
        }

        /// <summary>
        /// Handle custom back button click
        /// </summary>
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await NavigationService.NavigateBack();
        }
        
        /// <summary>
        /// Override hardware back button to use NavigationService which has the proper context
        /// to navigate back to EditGuestPage
        /// </summary>
        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await NavigationService.NavigateBack();
            });
            return true; // Prevent default back navigation to use our custom logic
        }
    }
}
