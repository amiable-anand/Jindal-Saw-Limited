using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;

namespace Jindal.Views
{
    [QueryProperty(nameof(RoomNumber), "roomNumber")]
    [QueryProperty(nameof(GuestId), "guestId")]
    [QueryProperty(nameof(SourcePage), "sourcePage")]
    public partial class AddGuestToSameRoomPage : ContentPage
    {
        public string RoomNumber { get; set; }
        public int GuestId { get; set; } // Only needed when returning to EditGuestPage
        public string SourcePage { get; set; }

        public AddGuestToSameRoomPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (!string.IsNullOrEmpty(RoomNumber))
                RoomNumberLabel.Text = $"Room: {RoomNumber}";
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GuestNameEntry.Text) ||
                string.IsNullOrWhiteSpace(GuestIdEntry.Text) ||
                IdTypePicker.SelectedItem == null)
            {
                await DisplayAlert("Validation Error", "Please fill all required fields.", "OK");
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
                NavigateBack();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
            }
        }

        protected override bool OnBackButtonPressed()
        {
            NavigateBack();
            return true; // prevent default behavior
        }

        private async void NavigateBack()
        {
            if (SourcePage == nameof(EditGuestPage))
            {
                await Shell.Current.GoToAsync($"{nameof(EditGuestPage)}?guestId={GuestId}");
            }
            else
            {
                await Shell.Current.GoToAsync(".."); // fallback to previous
            }
        }
    }
}
