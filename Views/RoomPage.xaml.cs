using Jindal.Models;
using Jindal.Services;
using System.Linq;

namespace Jindal.Views
{
    public partial class RoomPage : ContentPage
    {
        private List<Room> allRooms = new();

        public RoomPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadRooms();
        }

        private async Task LoadRooms()
        {
            allRooms = await DatabaseService.GetRooms();
            RoomCollection.ItemsSource = allRooms;
        }

        private async void OnAddRoomClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditRoomPage());
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Room room)
            {
                await Navigation.PushAsync(new AddEditRoomPage(room));
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Room room)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete Room {room.RoomNumber}?", "Yes", "No");
                if (confirm)
                {
                    await DatabaseService.DeleteRoom(room);
                    await LoadRooms();
                }
            }
        }

        private void OnSearchPressed(object sender, EventArgs e)
        {
            string query = SearchBar.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(query))
            {
                RoomCollection.ItemsSource = allRooms;
                return;
            }

            var filtered = allRooms.Where(r =>
                r.RoomNumber.ToString().Contains(query) ||
                (r.Location?.ToLower().Contains(query) ?? false) ||
                (r.Remark?.ToLower().Contains(query) ?? false)).ToList();

            RoomCollection.ItemsSource = filtered;
        }

        private async void OnReloadClicked(object sender, EventArgs e)
        {
            SearchBar.Text = string.Empty;
            await LoadRooms();
        }
    }
}
