using System;
using Jindal.Services;
using Microsoft.Maui.Controls;
using LocationModel = Jindal.Models.Location;

namespace Jindal.Views
{
    public partial class AddEditLocationPage : ContentPage
    {
        // Holds the current location for edit scenario
        private readonly LocationModel? currentLocation;

        // Constructor: Accepts an optional existing location for editing
        public AddEditLocationPage(LocationModel? location = null)
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
                string name = NameEntry.Text?.Trim() ?? string.Empty;
                string code = CodeEntry.Text?.Trim() ?? string.Empty;
                string address = AddressEntry.Text?.Trim() ?? string.Empty;
                string remark = RemarkEntry.Text?.Trim() ?? string.Empty;

                var locationToValidate = currentLocation ?? new LocationModel();
                locationToValidate.Name = name;
                locationToValidate.LocationCode = code;
                locationToValidate.Address = address;
                locationToValidate.Remark = remark;

                // Validate location data
                var validationResult = ValidationHelper.ValidateLocationData(locationToValidate);
                if (!validationResult.IsValid)
                {
                    await DisplayAlert("Validation Error", validationResult.GetErrorMessage(), "OK");
                    return;
                }

                // Execute with retry logic
                await ErrorHandlingService.ExecuteWithRetry(async () =>
                {
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
                        ErrorHandlingService.LogInfo($"New location added: {newLocation.Name}", "LocationManagement");
                    }
                    else
                    {
                        // Updating existing location
                        currentLocation.Name = name;
                        currentLocation.LocationCode = code;
                        currentLocation.Address = address;
                        currentLocation.Remark = remark;

                        await DatabaseService.UpdateLocation(currentLocation);
                        ErrorHandlingService.LogInfo($"Location updated: {currentLocation.Name} (ID={currentLocation.Id})", "LocationManagement");
                    }
                }, 3, "Location Save");

                await DisplayAlert("Success", "Location saved successfully.", "OK");

                // Pop this page asynchronously to return to previous page
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Error saving location", ex, "AddEditLocationPage");
                var userMessage = ErrorHandlingService.GetUserFriendlyErrorMessage(ex);
                await DisplayAlert("Error", userMessage, "OK");
            }
        }
    }
}
