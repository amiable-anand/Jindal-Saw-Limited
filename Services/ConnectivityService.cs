using Microsoft.Maui.Networking;
using System.Net.NetworkInformation;

namespace Jindal.Services
{
    public class ConnectivityService
    {
        private readonly ApiService _apiService;
        private bool _lastConnectionState = false;
        
        public ConnectivityService()
        {
            _apiService = new ApiService();
            
            // Subscribe to connectivity changes
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        public bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public async Task<bool> IsApiReachableAsync()
        {
            if (!IsConnected)
                return false;
                
            return await _apiService.IsApiAvailableAsync();
        }

        public async Task<bool> TestInternetConnectivity()
        {
            try
            {
                using var ping = new Ping();
                var result = await ping.SendPingAsync("8.8.8.8", 3000); // Google DNS
                return result.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            var currentState = e.NetworkAccess == NetworkAccess.Internet;
            
            if (currentState != _lastConnectionState)
            {
                _lastConnectionState = currentState;
                
                if (currentState)
                {
                    // Connection restored - trigger sync
                    await TriggerSync();
                }
                
                // Notify about connectivity change
                ConnectivityChanged?.Invoke(currentState);
            }
        }

        private async Task TriggerSync()
        {
            try
            {
                // Check if API is reachable before syncing
                if (await IsApiReachableAsync())
                {
                    // Note: PerformPendingSync removed for simplification
                    System.Diagnostics.Debug.WriteLine("API is reachable - sync would be triggered here");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during automatic sync: {ex.Message}");
            }
        }

        public event Action<bool>? ConnectivityChanged;

        public void Dispose()
        {
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
            _apiService?.Dispose();
        }
    }
}
