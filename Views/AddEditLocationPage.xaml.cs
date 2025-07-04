using System;
using Jindal.Services;
using Microsoft.Maui.Controls;
using LocationModel = Jindal.Models.Location;

namespace Jindal.Views
{
    public partial class AddEditLocationPage : ContentPage
    {
        // Holds the current location for edit scenario
        private readonly LocationModel currentLocation;

        // Constructor: Accepts an optional existing location for editing
        public AddEditLocationPage(LocationModel location = null)
        {
            InitializeComponent();
            currentLocation = location;

            // Populate the fields if editing an existing location
            if (currentLocation != null)
            {
                NameEntry.Text = currentLocation.Name;
                CodeEntry.Text = currentLocation.LocationCode;
                AddressEntry.Text = currentLocation.Address;
                RemarkEntry.Text = currentLocation.Remark;
            }
        }

        // Called when the "Save" button is clicked
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                string name = NameEntry.Text?.Trim();
                string code = CodeEntry.Text?.Trim();
                string address = AddressEntry.Text?.Trim();
                string remark = RemarkEntry.Text?.Trim();

                // Validate required fields
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
                {
                    await DisplayAlert("Missing Fields", "Location Name and Code are required.", "OK");
                    return;
                }

                if (currentLocation == null)
                {
                    // Adding a new location
                    var newLocation = new LocationModel
                    {
                        Name = name,
                        LocationCode = code,
                        Address = address,
                        Remark = remark
                    };

                    await DatabaseService.AddLocation(newLocation);
                    System.Diagnostics.Debug.WriteLine("New location added.");
                }
                else
                {
                    // Updating existing location
                    currentLocation.Name = name;
                    currentLocation.LocationCode = code;
                    currentLocation.Address = address;
                    currentLocation.Remark = remark;

                    await DatabaseService.UpdateLocation(currentLocation);
                    System.Diagnostics.Debug.WriteLine($"Location updated: ID={currentLocation.Id}");
                }

                await DisplayAlert("Success", "Location saved successfully.", "OK");

                // Pop this page asynchronously to return to previous page
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving location: {ex.Message}");
                await DisplayAlert("Error", "Something went wrong while saving the location.", "OK");
            }
        }
    }
}
