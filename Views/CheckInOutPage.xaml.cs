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
                await LoadData();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Clean up event handlers to prevent memory leaks
            RoomFilterPicker.SelectedIndexChanged -= OnRoomFilterChanged;
            SearchEntry.TextChanged -= OnSearchTextChanged;
        }

        /// <summary>
        /// Loads only active check-in records (not checked out) and populates filters and grid.
        /// </summary>
        private async Task LoadData()
        {
            try
            {
                // Initialize database connection
                await DatabaseService.Init();

                // Set up event handlers first
                RoomFilterPicker.SelectedIndexChanged -= OnRoomFilterChanged;
                RoomFilterPicker.SelectedIndexChanged += OnRoomFilterChanged;

                SearchEntry.TextChanged -= OnSearchTextChanged;
                SearchEntry.TextChanged += OnSearchTextChanged;

                // Get ONLY active guests (those who haven't checked out yet)
                allRecords = await DatabaseService.GetActiveGuests();
                
                // Validate data
                if (allRecords == null)
                {
                    allRecords = new List<CheckInOut>();
                }
                
                // Debug: Check if we have any data
                if (!allRecords.Any())
                {
                    await DisplayAlert("Info", "No active guests found. Checked out guests can be viewed in the Reports section.", "OK");
                }

                // Populate room filter with validation
                var roomNumbers = allRecords
                    .Where(r => r.RoomNumber > 0)
                    .Select(r => r.RoomNumber)
                    .Distinct()
                    .OrderBy(r => r)
                    .Select(r => r.ToString())
                    .ToList();
                    
                RoomFilterPicker.ItemsSource = roomNumbers;

                PopulateTable(allRecords);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load check-in data: {ex.Message}");
                await DisplayAlert("Database Error", $"Failed to load guest data: {ex.Message}", "OK");
                
                // Initialize empty data to prevent crashes
                allRecords = new List<CheckInOut>();
                PopulateTable(allRecords);
            }
        }

        /// <summary>
        /// Renders the table based on filtered or full guest records.
        /// </summary>
        private void PopulateTable(List<CheckInOut> records)
        {
            try
            {
                // Validate input
                if (records == null)
                {
                    records = new List<CheckInOut>();
                }

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
                    AddToGrid(new Label { Text = r.RoomNumber > 0 ? r.RoomNumber.ToString() : "-", TextColor = textColor }, 0, row);
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
                        await NavigationService.NavigateToEditGuest(r.Id);
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
                            await NavigationService.NavigateToCheckOut(r.Id);
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to populate table: {ex.Message}");
                // Don't show alert for table population errors as it might be called frequently
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
            await NavigationService.NavigateToAddGuest();
        }

        private async void OnReloadClicked(object sender, EventArgs e)
        {
            RoomFilterPicker.SelectedIndex = -1;
            SearchEntry.Text = string.Empty;
            await LoadData();
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            try
            {
                string search = e.NewTextValue?.ToLower()?.Trim() ?? "";

                if (string.IsNullOrEmpty(search))
                {
                    PopulateTable(allRecords);
                    return;
                }

                var filtered = allRecords
                    .Where(r => !string.IsNullOrEmpty(r.GuestName) && 
                               r.GuestName.ToLower().Contains(search))
                    .ToList();

                PopulateTable(filtered);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
                // Don't show alert for search errors, just log them
            }
        }

        private void OnRoomFilterChanged(object? sender, EventArgs e)
        {
            try
            {
                if (RoomFilterPicker.SelectedItem is not string selectedRoomStr)
                {
                    // If no selection, show all records
                    PopulateTable(allRecords);
                    return;
                }

                if (!int.TryParse(selectedRoomStr, out int selectedRoom))
                {
                    PopulateTable(allRecords);
                    return;
                }

                var filtered = allRecords
                    .Where(r => r.RoomNumber == selectedRoom)
                    .ToList();

                PopulateTable(filtered);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Room filter error: {ex.Message}");
                // Don't show alert for filter errors, just log them
                PopulateTable(allRecords); // Show all records if filter fails
            }
        }
    }
}
