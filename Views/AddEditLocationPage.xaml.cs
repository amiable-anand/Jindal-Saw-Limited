using System;
using Jindal.Services;
using Microsoft.Maui.Controls;
using LocationModel = Jindal.Models.Location;

namespace Jindal.Views
{
    public partial class AddEditLocationPage : ContentPage
    {
        private LocationModel currentLocation;

        public AddEditLocationPage(LocationModel location = null)
        {
            InitializeComponent();
            currentLocation = location;

            if (location != null)
            {
                NameEntry.Text = location.Name;
                CodeEntry.Text = location.LocationCode;
                AddressEntry.Text = location.Address;
                RemarkEntry.Text = location.Remark;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var name = NameEntry.Text?.Trim();
            var code = CodeEntry.Text?.Trim();
            var address = AddressEntry.Text?.Trim();
            var remark = RemarkEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
            {
                await DisplayAlert("Missing Fields", "Name and Code are required.", "OK");
                return;
            }

            if (currentLocation == null)
            {
                var newLocation = new LocationModel
                {
                    Name = name,
                    LocationCode = code,
                    Address = address,
                    Remark = remark
                };

                await DatabaseService.AddLocation(newLocation);
            }
            else
            {
                currentLocation.Name = name;
                currentLocation.LocationCode = code;
                currentLocation.Address = address;
                currentLocation.Remark = remark;

                await DatabaseService.UpdateLocation(currentLocation);
            }

            await DisplayAlert("Success", "Location saved successfully.", "OK");
            await Navigation.PopAsync();
        }
    }
}
