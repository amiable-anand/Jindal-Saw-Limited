using Jindal.Models;
using Jindal.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jindal.Views
{
    public partial class AddCheckInOutPage : ContentPage
    {
        public AddCheckInOutPage()
        {
            InitializeComponent();
            LoadAvailableRooms();
        }

        private async void LoadAvailableRooms()
        {
            var availableRooms = await DatabaseService.GetAvailableRooms(); // implement this in your service
            RoomPicker.ItemsSource = availableRooms;
        }

        private async void OnCheckInClicked(object sender, EventArgs e)
        {
            var newEntry = new CheckInOut
            {
                RoomNumber = RoomPicker.SelectedItem?.ToString(),
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
            await Navigation.PopAsync(); // Go back to the table
        }

        private void OnAddGuestClicked(object sender, EventArgs e)
        {
            // You can implement storing guest data without checking in yet
        }
    }
}
