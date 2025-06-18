using Jindal.Models;
using Jindal.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jindal.Views;

namespace Jindal.Views
{
    public partial class CheckInOutPage : ContentPage
    {
        private List<CheckInOut> allRecords = new();

        public CheckInOutPage()
        {
            InitializeComponent();

            // Hook up filters
            RoomFilterPicker.SelectedIndexChanged += OnRoomFilterChanged;
            SearchEntry.TextChanged += OnSearchTextChanged;
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

            // Set up room filter picker (convert nullable int to string safely)
            var roomNumbers = allRecords
                .Where(r => r.RoomNumber != null)
                .Select(r => r.RoomNumber.ToString()) // This is safe because RoomNumber is nullable
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            RoomFilterPicker.ItemsSource = roomNumbers;
        }


        private void PopulateTable(List<CheckInOut> records)
        {
            // Clear previous rows except headers
            while (CheckInOutTable.RowDefinitions.Count > 1)
                CheckInOutTable.RowDefinitions.RemoveAt(CheckInOutTable.RowDefinitions.Count - 1);

            var oldContent = CheckInOutTable.Children.Skip(10).ToList(); // Skip headers
            foreach (var view in oldContent)
                CheckInOutTable.Children.Remove(view);

            int row = 1;

            var groupedRecords = records
                .Where(r => r.RoomNumber != null)
                .GroupBy(r => r.RoomNumber)
                .OrderBy(g => g.Key);

            foreach (var group in groupedRecords)
            {
                // Header Row
                CheckInOutTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

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
                Grid.SetColumnSpan(roomHeader, 11); // span all columns including action
                Grid.SetRow(roomHeader, row);
                CheckInOutTable.Children.Add(roomHeader);
                row++;

                foreach (var r in group)
                {
                    CheckInOutTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    AddToGrid(new Label { Text = r.RoomNumber?.ToString() ?? "-", TextColor = Colors.White }, 0, row);
                    AddToGrid(new Label { Text = r.GuestName, TextColor = Colors.White }, 1, row);
                    AddToGrid(new Label { Text = r.GuestIdNumber, TextColor = Colors.White }, 2, row);
                    AddToGrid(new Label { Text = r.CheckInDate.ToString("dd-MM-yyyy"), TextColor = Colors.White }, 3, row);
                    AddToGrid(new Label { Text = r.CheckInTime.ToString(@"hh\:mm"), TextColor = Colors.White }, 4, row);
                    AddToGrid(new Label { Text = r.CheckOutDate?.ToString("dd-MM-yyyy") ?? "-", TextColor = Colors.White }, 5, row);
                    AddToGrid(new Label { Text = r.CheckOutTime?.ToString(@"hh\:mm") ?? "-", TextColor = Colors.White }, 6, row);
                    AddToGrid(new Label { Text = r.Department, TextColor = Colors.White }, 7, row);
                    AddToGrid(new Label { Text = r.Purpose, TextColor = Colors.White }, 8, row);
                    AddToGrid(new Label { Text = r.MailReceivedDate.ToString("dd-MM-yyyy"), TextColor = Colors.White }, 9, row);

                    // Right-aligned buttons
                    var buttonStack = new HorizontalStackLayout
                    {
                        Spacing = 6,
                        HorizontalOptions = LayoutOptions.End
                    };

                    var editButton = new Button
                    {
                        Text = "Edit",
                        FontSize = 12,
                        BackgroundColor = Color.FromArgb("#3B82F6"),
                        TextColor = Colors.White,
                        CornerRadius = 6,
                        Padding = new Thickness(8, 4)
                    };
                    editButton.Clicked += async (s, e) =>
                    {
                        await Shell.Current.GoToAsync($"///EditGuestPage?guestId={r.Id}");
                    };

                    var checkOutButton = new Button
                    {
                        Text = "Check Out",
                        FontSize = 12,
                        BackgroundColor = Color.FromArgb("#EF4444"),
                        TextColor = Colors.White,
                        CornerRadius = 6,
                        Padding = new Thickness(8, 4)
                    };
                    checkOutButton.Clicked += async (s, e) =>
                    {
                        r.CheckOutDate = DateTime.Now.Date;
                        r.CheckOutTime = DateTime.Now.TimeOfDay;
                        await DatabaseService.UpdateCheckInOut(r);
                        await LoadData();
                    };

                    buttonStack.Children.Add(editButton);
                    buttonStack.Children.Add(checkOutButton);

                    AddToGrid(buttonStack, 10, row);

                    row++;
                }

                CheckInOutTable.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
                row++;
            }
        }






        private void AddToGrid(View view, int col, int row)
        {
            Grid.SetColumn(view, col);
            Grid.SetRow(view, row);
            CheckInOutTable.Children.Add(view);
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AddCheckInOutPage));

        }

        private async void OnReloadClicked(object sender, EventArgs e)
        {
            RoomFilterPicker.SelectedIndex = -1;
            SearchEntry.Text = string.Empty;
            await LoadData();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string search = e.NewTextValue?.ToLower() ?? "";
            var filtered = allRecords
                .Where(r => r.GuestName?.ToLower().Contains(search) ?? false)
                .ToList();

            PopulateTable(filtered);
        }

        private void OnRoomFilterChanged(object sender, EventArgs e)
        {
            if (RoomFilterPicker.SelectedItem is not string selectedRoom)
                return;

            var filtered = allRecords
                .Where(r => r.RoomNumber?.ToString() == selectedRoom)
                .ToList();

            PopulateTable(filtered);
        }

    }
}
