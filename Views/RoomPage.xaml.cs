using Jindal.Models;
using Jindal.Services;

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
            var button = sender as Button;
            var room = button?.BindingContext as Room;

            if (room != null)
                await Navigation.PushAsync(new AddEditRoomPage(room));
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var room = button?.BindingContext as Room;

            if (room != null)
            {
                bool confirm = await DisplayAlert("Delete", $"Delete room {room.RoomNumber}?", "Yes", "No");
                if (confirm)
                {
                    await DatabaseService.DeleteRoom(room);
                    await LoadRooms();
                }
            }
        }

        private void OnSearchPressed(object sender, EventArgs e)
        {
            var query = SearchBar.Text?.ToLower();
            RoomCollection.ItemsSource = allRooms.Where(r =>
                r.RoomNumber.ToString().Contains(query) ||
                r.Location.ToLower().Contains(query) ||
                r.Remark.ToLower().Contains(query)).ToList();
        }

        private async void OnReloadClicked(object sender, EventArgs e)
        {
            await LoadRooms();
        }
    }
}
