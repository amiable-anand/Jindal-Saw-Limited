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

            try
            {
                // Ensure event handlers are not duplicated
                RoomFilterPicker.SelectedIndexChanged -= OnRoomFilterChanged;
                RoomFilterPicker.SelectedIndexChanged += OnRoomFilterChanged;

                SearchEntry.TextChanged -= OnSearchTextChanged;
                SearchEntry.TextChanged += OnSearchTextChanged;

                await LoadData();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Loads only active check-in records (not checked out) and populates filters and grid.
        /// </summary>
        private async Task LoadData()
        {
            try
            {
                await DatabaseService.Init();

                // Get ONLY active guests (those who haven't checked out yet)
                allRecords = await DatabaseService.GetActiveGuests();
                
                // Debug: Check if we have any data
                if (!allRecords.Any())
                {
                    await DisplayAlert("Info", "No active guests found. Checked out guests can be viewed in the Reports section.", "OK");
                }

                RoomFilterPicker.ItemsSource = allRecords
                    .Select(r => r.RoomNumber)
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();

                PopulateTable(allRecords);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Database Error", $"Failed to load guest data: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Renders the table based on filtered or full guest records.
        /// </summary>
        private void PopulateTable(List<CheckInOut> records)
        {
            // Clear existing rows beyond header
            while (CheckInOutTable.RowDefinitions.Count > 1)
                CheckInOutTable.RowDefinitions.RemoveAt(CheckInOutTable.RowDefinitions.Count - 1);

            var oldContent = CheckInOutTable.Children.Skip(11).ToList(); // Skip header cells
            foreach (var view in oldContent)
                CheckInOutTable.Children.Remove(view);

            int row = 1;

            foreach (var group in records.GroupBy(r => r.RoomNumber))
            {
                // Room Header Row
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
                Grid.SetColumnSpan(roomHeader, 11);
                Grid.SetRow(roomHeader, row);
                CheckInOutTable.Children.Add(roomHeader);
                row++;

                foreach (var r in group)
                {
                    CheckInOutTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    // All guests here are active (not checked out) since we filter in LoadData
                    var textColor = Color.FromArgb("#374151"); // Dark text for white background
                    
                    // Guest Data Row
                    AddToGrid(new Label { Text = r.RoomNumber ?? "-", TextColor = textColor }, 0, row);
                    AddToGrid(new Label { Text = r.GuestName ?? "-", TextColor = textColor }, 1, row);
                    AddToGrid(new Label { Text = r.GuestIdNumber ?? "-", TextColor = textColor }, 2, row);
                    AddToGrid(new Label { Text = r.CheckInDate != default ? r.CheckInDate.ToString("dd-MM-yyyy") : "-", TextColor = textColor }, 3, row);
                    AddToGrid(new Label { Text = r.CheckInTime != default ? r.CheckInTime.ToString(@"hh\:mm") : "-", TextColor = textColor }, 4, row);
                    
                    // For active guests, checkout columns will be empty
                    AddToGrid(new Label { Text = "-", TextColor = Color.FromArgb("#9CA3AF") }, 5, row);
                    AddToGrid(new Label { Text = "-", TextColor = Color.FromArgb("#9CA3AF") }, 6, row);
                    
                    AddToGrid(new Label { Text = r.Department ?? "-", TextColor = textColor }, 7, row);
                    AddToGrid(new Label { Text = r.Purpose ?? "-", TextColor = textColor }, 8, row);
                    AddToGrid(new Label { Text = r.MailReceivedDate != default ? r.MailReceivedDate.ToString("dd-MM-yyyy") : "-", TextColor = textColor }, 9, row);

                    // Action Buttons for active guests
                    var buttonStack = new HorizontalStackLayout
                    {
                        Spacing = 6,
                        HorizontalOptions = LayoutOptions.End
                    };

                    var editButton = new Button
                    {
                        Text = "Edit",
                        FontSize = 12,
                        BackgroundColor = Color.FromArgb("#1E3A8A"),
                        TextColor = Colors.White,
                        CornerRadius = 6,
                        FontAttributes = FontAttributes.Bold,
                        Padding = new Thickness(8, 4)
                    };
                    editButton.Clicked += async (s, e) =>
                    {
                        await Shell.Current.GoToAsync($"{nameof(EditGuestPage)}?guestId={r.Id}");
                    };
                    buttonStack.Children.Add(editButton);

                    // Check-out button for active guests
                    var checkOutButton = new Button
                    {
                        Text = "Check Out",
                        FontSize = 12,
                        BackgroundColor = Color.FromArgb("#DC2626"),
                        TextColor = Colors.White,
                        CornerRadius = 6,
                        FontAttributes = FontAttributes.Bold,
                        Padding = new Thickness(8, 4)
                    };
                    checkOutButton.Clicked += async (s, e) =>
                    {
                        try
                        {
                            await Shell.Current.GoToAsync($"{nameof(CheckOutPage)}?guestId={r.Id}");
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("Navigation Error", ex.Message, "OK");
                        }
                    };
                    buttonStack.Children.Add(checkOutButton);
                    
                    AddToGrid(buttonStack, 10, row);

                    row++;
                }

                // Add spacing row after group
                CheckInOutTable.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
                row++;
            }
        }

        /// <summary>
        /// Utility method to add any view to grid at given row/column.
        /// </summary>
        private void AddToGrid(View view, int col, int row)
        {
            if (view != null)
            {
                Grid.SetColumn(view, col);
                Grid.SetRow(view, row);
                CheckInOutTable.Children.Add(view);
            }
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

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            string search = e.NewTextValue?.ToLower() ?? "";

            var filtered = allRecords
                .Where(r => r.GuestName?.ToLower().Contains(search) ?? false)
                .ToList();

            PopulateTable(filtered);
        }

        private void OnRoomFilterChanged(object? sender, EventArgs e)
        {
            if (RoomFilterPicker.SelectedItem is not string selectedRoom)
                return;

            var filtered = allRecords
                .Where(r => r.RoomNumber == selectedRoom)
                .ToList();

            PopulateTable(filtered);
        }
    }
}
