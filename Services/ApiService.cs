using System.Net.Http;
using System.Net.Http.Json;
using Jindal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LocationModel = Jindal.Models.Location;

namespace Jindal.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService>? _logger;
        private readonly string _baseUrl;

        public ApiService(HttpClient? httpClient = null, ILogger<ApiService>? logger = null, IConfiguration? configuration = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _logger = logger;
            
            // Get base URL from configuration, defaulting to production API URL
            var baseUrl = configuration?["ApiSettings:BaseUrl"] ?? "http://localhost:5177/api/";
            _baseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
            
            // Configure basic HttpClient settings
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // Authentication
        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            try
            {
                var loginRequest = new { Username = username, Password = password };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}Users/authenticate", loginRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (loginResponse?.Token != null)
                    {
                        SetAuthToken(loginResponse.Token);
                    }
                    return loginResponse;
                }
                
                _logger?.LogWarning("Login failed with status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during login");
                return null;
            }
        }

        // Health Check
        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl.TrimEnd('/')}/health".Replace("/api/health", "/health"));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Users API
        public async Task<List<User>?> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Users");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<User>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting users from API");
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}Users", user);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating user via API");
                return false;
            }
        }

        // Rooms API
        public async Task<List<Room>?> GetRoomsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Rooms");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Room>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting rooms from API");
                return null;
            }
        }

        public async Task<List<Room>?> GetAvailableRoomsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Rooms/available");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Room>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting available rooms from API");
                return null;
            }
        }

        public async Task<bool> CreateRoomAsync(Room room)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}Rooms", room);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating room via API");
                return false;
            }
        }

        public async Task<bool> UpdateRoomAsync(Room room)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}Rooms/{room.Id}", room);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating room via API");
                return false;
            }
        }

        public async Task<bool> DeleteRoomAsync(int roomId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}Rooms/{roomId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting room via API");
                return false;
            }
        }

        // Locations API
        public async Task<List<LocationModel>?> GetLocationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Locations");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<LocationModel>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting locations from API");
                return null;
            }
        }

        public async Task<bool> CreateLocationAsync(LocationModel location)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}Locations", location);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating location via API");
                return false;
            }
        }

        public async Task<bool> UpdateLocationAsync(LocationModel location)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}Locations/{location.Id}", location);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating location via API");
                return false;
            }
        }

        public async Task<bool> DeleteLocationAsync(int locationId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}Locations/{locationId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting location via API");
                return false;
            }
        }

        // CheckInOut API
        public async Task<List<CheckInOut>?> GetCheckInOutsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}CheckInOut");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<CheckInOut>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting check-in/outs from API");
                return null;
            }
        }

        public async Task<bool> CreateCheckInOutAsync(CheckInOut checkInOut)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}CheckInOut", checkInOut);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating check-in/out via API");
                return false;
            }
        }

        public async Task<bool> UpdateCheckInOutAsync(CheckInOut checkInOut)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}CheckInOut/{checkInOut.Id}", checkInOut);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating check-in/out via API");
                return false;
            }
        }

        public async Task<bool> DeleteCheckInOutAsync(int checkInOutId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}CheckInOut/{checkInOutId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting check-in/out via API");
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // DTO Classes for API responses
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public ApiUser? User { get; set; }
    }

    public class ApiUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int Permissions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string PermissionText { get; set; } = string.Empty;
    }
    
}
