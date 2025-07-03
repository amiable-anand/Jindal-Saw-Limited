using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jindal.Views
{
    [QueryProperty(nameof(GuestId), "guestId")]
    public partial class EditGuestPage : ContentPage
    {
        public int GuestId { get; set; }

        private CheckInOut currentGuest;
        private List<CheckInOut> roomGuests = new();

        public EditGuestPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadGuestDetails();
        }

        private async Task LoadGuestDetails()
        {
            await DatabaseService.Init();

            currentGuest = await DatabaseService.GetCheckInOutById(GuestId);

            if (currentGuest == null)
            {
                await DisplayAlert("Error", "Guest not found", "OK");
                await Shell.Current.GoToAsync("//CheckInOutPage");
                return;
            }

            // Fill fields
            GuestNameEntry.Text = currentGuest.GuestName;
            GuestIdEntry.Text = currentGuest.GuestIdNumber;
            DepartmentEntry.Text = currentGuest.Department;
            PurposeEntry.Text = currentGuest.Purpose;
            CheckInDatePicker.Date = currentGuest.CheckInDate;
            CheckInTimePicker.Time = currentGuest.CheckInTime;
            RoomLabel.Text = currentGuest.RoomNumber;

            // Load all guests in the same room
            roomGuests = await DatabaseService.GetCheckInOutsByRoomNumber(currentGuest.RoomNumber);
            PopulateRoomGuestTable(roomGuests);
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                currentGuest.GuestName = GuestNameEntry.Text;
                currentGuest.GuestIdNumber = GuestIdEntry.Text;
                currentGuest.Department = DepartmentEntry.Text;
                currentGuest.Purpose = PurposeEntry.Text;
                currentGuest.CheckInDate = CheckInDatePicker.Date;
                currentGuest.CheckInTime = CheckInTimePicker.Time;

                await DatabaseService.UpdateCheckInOut(currentGuest);
                await DisplayAlert("Success", "Guest updated successfully.", "OK");

                // Navigate explicitly to CheckInOutPage
                await Shell.Current.GoToAsync("//CheckInOutPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Update failed: {ex.Message}", "OK");
            }
        }

        private void PopulateRoomGuestTable(List<CheckInOut> guests)
        {
            RoomGuestsTable.Children.Clear();
            RoomGuestsTable.RowDefinitions.Clear();

            // Add header row
            RoomGuestsTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddToGrid(new Label { Text = "Name", FontAttributes = FontAttributes.Bold }, 0, 0);
            AddToGrid(new Label { Text = "Date", FontAttributes = FontAttributes.Bold }, 1, 0);
            AddToGrid(new Label { Text = "Time", FontAttributes = FontAttributes.Bold }, 2, 0);
            AddToGrid(new Label { Text = "Purpose", FontAttributes = FontAttributes.Bold }, 3, 0);
            AddToGrid(new Label { Text = "Department", FontAttributes = FontAttributes.Bold }, 4, 0);

            int row = 1;
            foreach (var g in guests)
            {
                RoomGuestsTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                AddToGrid(new Label { Text = g.GuestName ?? "-" }, 0, row);
                AddToGrid(new Label { Text = g.CheckInDate != default ? g.CheckInDate.ToString("dd-MM-yyyy") : "-" }, 1, row);
                AddToGrid(new Label { Text = g.CheckInTime != default ? g.CheckInTime.ToString(@"hh\:mm") : "-" }, 2, row);
                AddToGrid(new Label { Text = string.IsNullOrWhiteSpace(g.Purpose) ? "-" : g.Purpose }, 3, row);
                AddToGrid(new Label { Text = string.IsNullOrWhiteSpace(g.Department) ? "-" : g.Department }, 4, row);

                row++;
            }
        }

        private void AddToGrid(View view, int col, int row)
        {
            try
            {
                if (view != null)
                {
                    Grid.SetColumn(view, col);
                    Grid.SetRow(view, row);
                    RoomGuestsTable.Children.Add(view);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Grid Error] Row={row} Col={col} Message={ex.Message}");
            }
        }

        private async void OnAddGuestToSameRoomClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(AddGuestToSameRoomPage)}?roomNumber={currentGuest.RoomNumber}&guestId={currentGuest.Id}&sourcePage={nameof(EditGuestPage)}");
        }

        // ?? Custom Back Button override
        protected override bool OnBackButtonPressed()
        {
            // Navigate directly to CheckInOutPage
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("//CheckInOutPage");
            });

            return true; // ? We handled the back button
        }
    }
}
