using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;

namespace Jindal.Views
{
    [QueryProperty(nameof(RoomNumber), "roomNumber")]
    public partial class AddGuestToSameRoomPage : ContentPage
    {
        private string _roomNumber;

        public string RoomNumber
        {
            get => _roomNumber;
            set
            {
                _roomNumber = value;
                if (!string.IsNullOrEmpty(_roomNumber))
                {
                    RoomNumberLabel.Text = $"Room: {_roomNumber}";
                }
            }
        }

        public AddGuestToSameRoomPage()
        {
            InitializeComponent();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // Basic validation
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
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
            }
        }
    }
}
