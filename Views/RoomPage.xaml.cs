using Jindal.Models;
using Jindal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Jindal.Views
{
    public partial class RoomPage : ContentPage
    {
        // ?? Holds all rooms for search/reset
        private List<Room> allRooms = new();

        public RoomPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Triggered every time the page appears. Loads fresh data.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadRoomsAsync();
        }

        /// <summary>
        /// Loads rooms and location names from the database.
        /// Sets the ItemsSource for the CollectionView.
        /// </summary>
        private async Task LoadRoomsAsync()
        {
            await DatabaseService.Init();

            var locations = await DatabaseService.GetLocations();
            allRooms = await DatabaseService.GetRooms();

            // Set LocationName for each room
            foreach (var room in allRooms)
            {
                var loc = locations.FirstOrDefault(l => l.Id == room.LocationId);
                room.LocationName = loc?.Name ?? "Unknown";
            }

            RoomCollection.ItemsSource = allRooms;
        }

        /// <summary>
        /// Navigates to Add Room screen.
        /// </summary>
        private async void OnAddRoomClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditRoomPage());
        }

        /// <summary>
        /// Navigates to Edit Room screen with selected room.
        /// </summary>
        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Room room)
            {
                await Navigation.PushAsync(new AddEditRoomPage(room));
            }
        }

        /// <summary>
        /// Deletes a room after user confirmation.
        /// </summary>
        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Room room)
            {
                bool confirm = await DisplayAlert(
                    "Confirm Delete",
                    $"Are you sure you want to delete Room {room.RoomNumber}?",
                    "Yes", "No");

                if (confirm)
                {
                    await DatabaseService.DeleteRoom(room);
                    await LoadRoomsAsync(); // Refresh list
                }
            }
        }

        /// <summary>
        /// Filters room list based on search text.
        /// </summary>
        private void OnSearchPressed(object sender, EventArgs e)
        {
            string query = SearchBar.Text?.Trim().ToLower() ?? string.Empty;

            if (string.IsNullOrEmpty(query))
            {
                RoomCollection.ItemsSource = allRooms;
                return;
            }

            var filtered = allRooms.Where(r =>
                r.RoomNumber.ToString().Contains(query) ||
                (r.LocationName?.ToLower().Contains(query) ?? false) ||
                (r.Remark?.ToLower().Contains(query) ?? false)).ToList();

            RoomCollection.ItemsSource = filtered;
        }

        /// <summary>
        /// Reloads the full list and clears search input.
        /// </summary>
        private async void OnReloadClicked(object sender, EventArgs e)
        {
            SearchBar.Text = string.Empty;
            await LoadRoomsAsync();
        }
    }
}
