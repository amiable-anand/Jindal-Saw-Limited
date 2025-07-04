using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
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
            await LoadGuestDetailsAsync();
        }

        /// <summary>
        /// Loads guest and related room guest details by GuestId.
        /// </summary>
        private async Task LoadGuestDetailsAsync()
        {
            await DatabaseService.Init();

            currentGuest = await DatabaseService.GetCheckInOutById(GuestId);

            if (currentGuest == null)
            {
                await DisplayAlert("Error", "Guest not found", "OK");
                await Shell.Current.GoToAsync("//CheckInOutPage");
                return;
            }

            // Populate UI fields
            GuestNameEntry.Text = currentGuest.GuestName;
            GuestIdEntry.Text = currentGuest.GuestIdNumber;
            DepartmentEntry.Text = currentGuest.Department;
            PurposeEntry.Text = currentGuest.Purpose;
            CheckInDatePicker.Date = currentGuest.CheckInDate;
            CheckInTimePicker.Time = currentGuest.CheckInTime;
            RoomLabel.Text = currentGuest.RoomNumber;

            // Load guests in the same room
            roomGuests = await DatabaseService.GetCheckInOutsByRoomNumber(currentGuest.RoomNumber);
            PopulateRoomGuestTable(roomGuests);
        }

        /// <summary>
        /// Save updated guest info to the database.
        /// </summary>
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (currentGuest == null)
            {
                await DisplayAlert("Error", "No guest loaded to update.", "OK");
                return;
            }

            try
            {
                currentGuest.GuestName = GuestNameEntry.Text?.Trim();
                currentGuest.GuestIdNumber = GuestIdEntry.Text?.Trim();
                currentGuest.Department = DepartmentEntry.Text?.Trim();
                currentGuest.Purpose = PurposeEntry.Text?.Trim();
                currentGuest.CheckInDate = CheckInDatePicker.Date;
                currentGuest.CheckInTime = CheckInTimePicker.Time;

                await DatabaseService.UpdateCheckInOut(currentGuest);
                await DisplayAlert("Success", "Guest updated successfully.", "OK");
                await Shell.Current.GoToAsync("//CheckInOutPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Update failed: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Populates the grid with guests in the same room.
        /// </summary>
        private void PopulateRoomGuestTable(List<CheckInOut> guests)
        {
            RoomGuestsTable.Children.Clear();
            RoomGuestsTable.RowDefinitions.Clear();

            // Add header row
            RoomGuestsTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AddToGrid(new Label { Text = "Name", FontAttributes = FontAttributes.Bold, TextColor = Colors.White }, 0, 0);
            AddToGrid(new Label { Text = "Date", FontAttributes = FontAttributes.Bold, TextColor = Colors.White }, 1, 0);
            AddToGrid(new Label { Text = "Time", FontAttributes = FontAttributes.Bold, TextColor = Colors.White }, 2, 0);
            AddToGrid(new Label { Text = "Purpose", FontAttributes = FontAttributes.Bold, TextColor = Colors.White }, 3, 0);
            AddToGrid(new Label { Text = "Department", FontAttributes = FontAttributes.Bold, TextColor = Colors.White }, 4, 0);

            int row = 1;
            foreach (var guest in guests)
            {
                RoomGuestsTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                AddToGrid(new Label { Text = guest.GuestName ?? "-", TextColor = Colors.LightGray }, 0, row);
                AddToGrid(new Label { Text = guest.CheckInDate.ToString("dd-MM-yyyy"), TextColor = Colors.LightGray }, 1, row);
                AddToGrid(new Label { Text = guest.CheckInTime.ToString(@"hh\:mm"), TextColor = Colors.LightGray }, 2, row);
                AddToGrid(new Label { Text = string.IsNullOrWhiteSpace(guest.Purpose) ? "-" : guest.Purpose, TextColor = Colors.LightGray }, 3, row);
                AddToGrid(new Label { Text = string.IsNullOrWhiteSpace(guest.Department) ? "-" : guest.Department, TextColor = Colors.LightGray }, 4, row);

                row++;
            }
        }

        /// <summary>
        /// Adds a view to the grid at the specified column and row.
        /// </summary>
        private void AddToGrid(View view, int col, int row)
        {
            try
            {
                Grid.SetColumn(view, col);
                Grid.SetRow(view, row);
                RoomGuestsTable.Children.Add(view);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Grid Error] Row={row} Col={col} Message={ex.Message}");
            }
        }

        /// <summary>
        /// Navigate to AddGuestToSameRoomPage for current room.
        /// </summary>
        private async void OnAddGuestToSameRoomClicked(object sender, EventArgs e)
        {
            if (currentGuest == null)
                return;

            await Shell.Current.GoToAsync($"{nameof(AddGuestToSameRoomPage)}?roomNumber={currentGuest.RoomNumber}&guestId={currentGuest.Id}&sourcePage={nameof(EditGuestPage)}");
        }

        /// <summary>
        /// Override back button to navigate explicitly to CheckInOutPage.
        /// </summary>
        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("//CheckInOutPage");
            });
            return true; // Prevent default back navigation
        }
    }
}
