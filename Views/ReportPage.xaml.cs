using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jindal.Views
{
    public partial class ReportPage : ContentPage
    {
        private List<CheckInOut> allCheckedOutRecords = new();

        public ReportPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await LoadCheckedOutData();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task LoadCheckedOutData()
        {
            await DatabaseService.Init();
            var allRecords = await DatabaseService.GetCheckInOuts();

            allCheckedOutRecords = allRecords
                .Where(r => r.CheckOutDate != null && r.CheckOutTime != null)
                .OrderByDescending(r => r.CheckOutDate)
                .ToList();

            // Populate filter dropdown
            RoomFilterPicker.ItemsSource = allCheckedOutRecords
                .Select(r => r.RoomNumber)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string selectedRoom = RoomFilterPicker.SelectedItem as string;
            string search = SearchEntry.Text?.ToLower() ?? "";

            var filtered = allCheckedOutRecords
                .Where(r =>
                    (string.IsNullOrEmpty(selectedRoom) || r.RoomNumber == selectedRoom) &&
                    (string.IsNullOrEmpty(search) || r.GuestName?.ToLower().Contains(search) == true))
                .ToList();

            PopulateTable(filtered);
        }

        private void PopulateTable(List<CheckInOut> records)
        {
            while (ReportTable.RowDefinitions.Count > 1)
                ReportTable.RowDefinitions.RemoveAt(ReportTable.RowDefinitions.Count - 1);

            var oldContent = ReportTable.Children.Skip(11).ToList(); // Skip headers
            foreach (var view in oldContent)
                ReportTable.Children.Remove(view);

            int row = 1;

            foreach (var group in records.GroupBy(r => r.RoomNumber))
            {
                ReportTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var roomHeader = new Label
                {
                    Text = $"Room {group.Key}",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 14,
                    TextColor = Colors.LightBlue,
                    BackgroundColor = Color.FromArgb("#222"),
                    Padding = new Thickness(8),
                };
                Grid.SetColumn(roomHeader, 0);
                Grid.SetColumnSpan(roomHeader, 11);
                Grid.SetRow(roomHeader, row);
                ReportTable.Children.Add(roomHeader);
                row++;

                foreach (var r in group)
                {
                    ReportTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    AddToGrid(new Label { Text = r.RoomNumber ?? "-", TextColor = Colors.White }, 0, row);
                    AddToGrid(new Label { Text = r.GuestName ?? "-", TextColor = Colors.White }, 1, row);
                    AddToGrid(new Label { Text = r.GuestIdNumber ?? "-", TextColor = Colors.White }, 2, row);
                    AddToGrid(new Label { Text = r.CheckInDate.ToString("dd-MM-yyyy"), TextColor = Colors.White }, 3, row);
                    AddToGrid(new Label { Text = r.CheckInTime.ToString(@"hh\:mm"), TextColor = Colors.White }, 4, row);
                    AddToGrid(new Label { Text = r.CheckOutDate?.ToString("dd-MM-yyyy") ?? "-", TextColor = Colors.White }, 5, row);
                    AddToGrid(new Label { Text = r.CheckOutTime?.ToString(@"hh\:mm") ?? "-", TextColor = Colors.White }, 6, row);
                    AddToGrid(new Label { Text = r.Department ?? "-", TextColor = Colors.White }, 7, row);
                    AddToGrid(new Label { Text = r.Purpose ?? "-", TextColor = Colors.White }, 8, row);
                    AddToGrid(new Label { Text = r.MailReceivedDate.ToString("dd-MM-yyyy"), TextColor = Colors.White }, 9, row);

                    var dummyView = new Label { Text = "--", TextColor = Colors.Gray };
                    AddToGrid(dummyView, 10, row);

                    row++;
                }

                ReportTable.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
                row++;
            }
        }

        private void AddToGrid(View view, int col, int row)
        {
            Grid.SetColumn(view, col);
            Grid.SetRow(view, row);
            ReportTable.Children.Add(view);
        }

        private void OnRoomFilterChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private async void OnReloadClicked(object sender, EventArgs e)
        {
            RoomFilterPicker.SelectedIndex = -1;
            SearchEntry.Text = string.Empty;
            await LoadCheckedOutData();
        }
    }
}
