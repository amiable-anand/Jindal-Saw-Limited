using Jindal.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Jindal.Views
{
    public partial class DashboardPage : ContentPage
    {
        public ObservableCollection<ActivityItem> RecentActivities { get; set; }

        public DashboardPage()
        {
            InitializeComponent();
            RecentActivities = new ObservableCollection<ActivityItem>();
            RecentActivityCollection.ItemsSource = RecentActivities;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDashboardData();
            await CreateTestDataIfNeeded();
            UpdateDateTime();
            LoadWelcomeMessage();
        }

        private async Task LoadDashboardData()
        {
            try
            {
                // Initialize database first
                await DatabaseService.Init();
                
                // Load room statistics
                var allRooms = await DatabaseService.GetRooms();
                var availableRooms = await DatabaseService.GetAvailableRooms();
                var allGuests = await DatabaseService.GetCheckInOuts();
                var activeGuests = allGuests.Where(g => g.CheckOutDate == null).ToList();

                // Update UI with real data or defaults
                TotalRoomsLabel.Text = allRooms?.Count.ToString() ?? "0";
                AvailableRoomsLabel.Text = availableRooms?.Count.ToString() ?? "0";
                OccupiedRoomsLabel.Text = activeGuests?.Count.ToString() ?? "0";
                ActiveGuestsLabel.Text = activeGuests?.Count.ToString() ?? "0";

                // Load recent activities
                await LoadRecentActivities();
                
                // Debug info
                System.Diagnostics.Debug.WriteLine($"Dashboard loaded: {allRooms?.Count} rooms, {activeGuests?.Count} active guests");
            }
            catch (Exception ex)
            {
                // Set default values on error
                TotalRoomsLabel.Text = "Error";
                AvailableRoomsLabel.Text = "Error";
                OccupiedRoomsLabel.Text = "Error";
                ActiveGuestsLabel.Text = "Error";
                
                await DisplayAlert("Dashboard Error", $"Failed to load dashboard data: {ex.Message}\n\nPlease check your database connection.", "OK");
                System.Diagnostics.Debug.WriteLine($"Dashboard error: {ex}");
            }
        }

        private async Task LoadRecentActivities()
        {
            try
            {
                RecentActivities.Clear();
                
                var recentCheckIns = await DatabaseService.GetCheckInOuts();
                var todayCheckIns = recentCheckIns
                    .Where(c => c.CheckInDate.Date == DateTime.Today)
                    .OrderByDescending(c => c.CheckInDate)
                    .Take(5);

                foreach (var checkIn in todayCheckIns)
                {
                    var isCheckOut = checkIn.CheckOutDate.HasValue;
                    RecentActivities.Add(new ActivityItem
                    {
                        Icon = isCheckOut ? "🚪" : "🏠",
                        Message = isCheckOut 
                            ? $"{checkIn.GuestName} checked out from Room {checkIn.RoomNumber}"
                            : $"{checkIn.GuestName} checked in to Room {checkIn.RoomNumber}",
                        Time = checkIn.CheckInDate.ToString("HH:mm")
                    });
                }

                if (!RecentActivities.Any())
                {
                    RecentActivities.Add(new ActivityItem
                    {
                        Icon = "ℹ️",
                        Message = "No recent activity today",
                        Time = "—"
                    });
                }
            }
            catch (Exception ex)
            {
                RecentActivities.Add(new ActivityItem
                {
                    Icon = "❌",
                    Message = "Failed to load recent activities",
                    Time = DateTime.Now.ToString("HH:mm")
                });
            }
        }

        private void UpdateDateTime()
        {
            CurrentDateTimeLabel.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm");
        }

        private void LoadWelcomeMessage()
        {
            var userCode = Preferences.Get("UserCode", "Admin");
            WelcomeUserLabel.Text = $"Welcome back, {userCode}!";
        }

        // Quick Action Handlers
        private async void OnNewCheckInClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//CheckInOutPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }

        private async void OnAddRoomClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//RoomPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }

        private async void OnViewReportsClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//ReportPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }

        private async void OnManageLocationsClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//LocationPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadDashboardData();
            
            // Show database stats for debugging
            var stats = await DatabaseService.GetDatabaseStats();
            var isConnected = await DatabaseService.TestDatabaseConnection();
            
            await DisplayAlert("Refreshed", 
                $"Dashboard data has been refreshed!\n\nDatabase Status: {(isConnected ? "Connected" : "Disconnected")}\n{stats}", 
                "OK");
        }
        
        // Helper method to create test data if database is empty
        private async Task CreateTestDataIfNeeded()
        {
            try
            {
                var rooms = await DatabaseService.GetRooms();
                var guests = await DatabaseService.GetCheckInOuts();
                
                // If no data exists, offer to create test data
                if (!rooms.Any() && !guests.Any())
                {
                    var createTest = await DisplayAlert("No Data Found", 
                        "No guest or room data found. Would you like to create some test data to see the dashboard in action?", 
                        "Yes", "No");
                        
                    if (createTest)
                    {
                        // This will trigger the default data creation in DatabaseService
                        await DatabaseService.Init();
                        await LoadDashboardData();
                        await DisplayAlert("Test Data Created", "Sample rooms and locations have been created. You can now add guests!", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating test data: {ex}");
            }
        }
    }

    public class ActivityItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}
