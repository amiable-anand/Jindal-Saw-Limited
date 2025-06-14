using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jindal.Views
{
    public partial class CheckInOutPage : ContentPage
    {
        private List<CheckInOut> allRecords = new();

        public CheckInOutPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private async Task LoadData()
        {
            allRecords = await DatabaseService.GetCheckInOuts();
            PopulateTable(allRecords);
        }

        private void PopulateTable(List<CheckInOut> records)
        {
            // Clear previous rows (but keep header row)
            while (CheckInOutTable.RowDefinitions.Count > 1)
                CheckInOutTable.RowDefinitions.RemoveAt(CheckInOutTable.RowDefinitions.Count - 1);

            var viewsToRemove = CheckInOutTable.Children.Skip(10).ToList();
            foreach (var view in viewsToRemove)
                CheckInOutTable.Children.Remove(view);

            int row = 1;
            foreach (var r in records)
            {
                CheckInOutTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                AddToGrid(new Label { Text = r.RoomNumber.ToString(), TextColor = Colors.Black }, 0, row);
                AddToGrid(new Label { Text = r.GuestName, TextColor = Colors.Black }, 1, row);
                AddToGrid(new Label { Text = r.GuestIdNumber, TextColor = Colors.Black }, 2, row);
                AddToGrid(new Label { Text = r.CheckInDate.ToString("dd-MM-yyyy"), TextColor = Colors.Black }, 3, row);
                AddToGrid(new Label { Text = r.CheckInTime.ToString(@"hh\:mm"), TextColor = Colors.Black }, 4, row);
                AddToGrid(new Label { Text = r.CheckOutDate?.ToString("dd-MM-yyyy") ?? "-", TextColor = Colors.Black }, 5, row);
                AddToGrid(new Label { Text = r.CheckOutTime?.ToString(@"hh\:mm") ?? "-", TextColor = Colors.Black }, 6, row);
                AddToGrid(new Label { Text = r.Department, TextColor = Colors.Black }, 7, row);
                AddToGrid(new Label { Text = r.Purpose, TextColor = Colors.Black }, 8, row);
                AddToGrid(new Label { Text = r.MailReceivedDate.ToString("dd-MM-yyyy"), TextColor = Colors.Black }, 9, row);

                row++;
            }
        }

        private void AddToGrid(View view, int col, int row)
        {
            Grid.SetColumn(view, col);
            Grid.SetRow(view, row);
            CheckInOutTable.Children.Add(view);
        }

        private async void OnReloadClicked(object sender, EventArgs e)
        {
            SearchEntry.Text = string.Empty;
            RoomFilterPicker.SelectedIndex = -1;
            await LoadData();
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Coming Soon", "Check-in/out form is not implemented yet.", "OK");
        }
    }
}
