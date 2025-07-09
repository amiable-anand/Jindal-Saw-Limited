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
            
            // Perform system health check
            await ErrorHandlingService.ValidateSystemHealth();
            
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
                
                // Load room statistics using DatabaseService
                var roomStats = await DatabaseService.GetRoomUtilizationStats();

                // Update UI with real data
                TotalRoomsLabel.Text = roomStats.TotalRooms.ToString();
                AvailableRoomsLabel.Text = roomStats.AvailableRooms.ToString();
                OccupiedRoomsLabel.Text = roomStats.OccupiedRooms.ToString();
                ActiveGuestsLabel.Text = roomStats.TotalActiveGuests.ToString();

                // Load recent activities
                await LoadRecentActivities();
                
                // Debug info
                System.Diagnostics.Debug.WriteLine($"Dashboard loaded: {roomStats.TotalRooms} rooms, {roomStats.TotalActiveGuests} active guests");
            }
            catch (Exception ex)
            {
                // Set default values on error
                TotalRoomsLabel.Text = "Error";
                AvailableRoomsLabel.Text = "Error";
                OccupiedRoomsLabel.Text = "Error";
                ActiveGuestsLabel.Text = "Error";
                
                ErrorHandlingService.LogError("Failed to load dashboard data", ex, "DashboardPage");
                var userMessage = ErrorHandlingService.GetUserFriendlyErrorMessage(ex);
                await DisplayAlert("Dashboard Error", userMessage, "OK");
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
                System.Diagnostics.Debug.WriteLine($"Failed to load recent activities: {ex.Message}");
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
            var userFullName = Preferences.Get("CurrentUserFullName", "Guest");
            var username = Preferences.Get("CurrentUserUsername", "Unknown");
            WelcomeUserLabel.Text = $"Welcome back, {userFullName}!";
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
