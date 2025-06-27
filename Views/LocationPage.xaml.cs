using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationModel = Jindal.Models.Location;

namespace Jindal.Views
{
    public partial class LocationPage : ContentPage
    {
        private List<LocationModel> allLocations = new();

        public LocationPage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            await DatabaseService.Init();
            allLocations = await DatabaseService.GetLocations();
            LocationCollection.ItemsSource = allLocations;
        }

        private async void OnAddLocationClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditLocationPage(null));
        }

        private async void OnReloadClicked(object sender, EventArgs e)
        {
            SearchBar.Text = string.Empty;
            allLocations = await DatabaseService.GetLocations();
            LocationCollection.ItemsSource = allLocations;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            string query = SearchBar.Text?.Trim() ?? "";

            allLocations = string.IsNullOrWhiteSpace(query)
                ? await DatabaseService.GetLocations()
                : await DatabaseService.SearchLocations(query);

            LocationCollection.ItemsSource = allLocations;
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is LocationModel loc)
            {
                await Navigation.PushAsync(new AddEditLocationPage(loc));
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is LocationModel loc)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete '{loc.Name}'?", "Yes", "No");
                if (confirm)
                {
                    await DatabaseService.DeleteLocation(loc);
                    allLocations = await DatabaseService.GetLocations();
                    LocationCollection.ItemsSource = allLocations;
                }
            }
        }
    }
}
