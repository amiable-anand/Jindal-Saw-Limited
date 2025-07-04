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
            LoadLocationsAsync();
        }

        /// <summary>
        /// Loads all location records from the database and populates the CollectionView.
        /// </summary>
        private async void LoadLocationsAsync()
        {
            try
            {
                await DatabaseService.Init();
                allLocations = await DatabaseService.GetLocations();
                LocationCollection.ItemsSource = allLocations;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load locations: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Opens the Add/Edit page with a null model to add a new location.
        /// </summary>
        private async void OnAddLocationClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditLocationPage(null));
        }

        /// <summary>
        /// Reloads the full list of locations and clears search input.
        /// </summary>
        private async void OnReloadClicked(object sender, EventArgs e)
        {
            try
            {
                SearchBar.Text = string.Empty;
                allLocations = await DatabaseService.GetLocations();
                LocationCollection.ItemsSource = allLocations;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Reload Failed", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Filters the location list based on the search bar text.
        /// </summary>
        private async void OnSearchClicked(object sender, EventArgs e)
        {
            try
            {
                string query = SearchBar.Text?.Trim() ?? "";

                allLocations = string.IsNullOrWhiteSpace(query)
                    ? await DatabaseService.GetLocations()
                    : await DatabaseService.SearchLocations(query);

                LocationCollection.ItemsSource = allLocations;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Search Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Opens the Add/Edit page with the selected location data for editing.
        /// </summary>
        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is LocationModel location)
            {
                await Navigation.PushAsync(new AddEditLocationPage(location));
            }
        }

        /// <summary>
        /// Deletes the selected location after confirmation and refreshes the list.
        /// </summary>
        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is LocationModel location)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete '{location.Name}'?", "Yes", "No");
                if (confirm)
                {
                    try
                    {
                        await DatabaseService.DeleteLocation(location);
                        allLocations = await DatabaseService.GetLocations();
                        LocationCollection.ItemsSource = allLocations;
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Delete Failed", ex.Message, "OK");
                    }
                }
            }
        }
    }
}
