using SQLite;
using Jindal.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocationModel = Jindal.Models.Location;
using BCrypt.Net;

namespace Jindal.Services
{
    /// <summary>
    /// Service class to manage all database operations using SQLite.
    /// </summary>
    public static class DatabaseService
    {
        private static SQLiteAsyncConnection? _database;

        /// <summary>
        /// Initializes the database and ensures tables and seed data exist.
        /// </summary>
        public static async Task Init()
        {
            if (_database != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Jindal.db");
            _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<Room>();
        await _database.CreateTableAsync<CheckInOut>();
        await _database.CreateTableAsync<LocationModel>();
        await _database.CreateTableAsync<User>();

        await EnsureDefaultUserAsync();
        await EnsureDefaultLocationsAsync();
        await EnsureDemoRoomsAsync();
        }


        private static async Task EnsureDefaultLocationsAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            var existingLocations = await _database.Table<LocationModel>().CountAsync();
            if (existingLocations == 0)
            {
                // Add some demo locations for testing
                await _database.InsertAllAsync(new[]
                {
                    new LocationModel { Name = "Main Building", LocationCode = "MB", Address = "123 Main St" },
                    new LocationModel { Name = "Guest House", LocationCode = "GH", Address = "456 Guest Ave" },
                    new LocationModel { Name = "Conference Center", LocationCode = "CC", Address = "789 Conference Blvd" }
                });
            }
        }

        private static async Task EnsureDemoRoomsAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            var existingRooms = await _database.Table<Room>().CountAsync();
            if (existingRooms == 0)
            {
                // Ensure we have locations first
                var locations = await _database.Table<LocationModel>().ToListAsync();
                if (locations.Any())
                {
                    var mainBuildingId = locations.First().Id;
                    
                    // Add some demo rooms for testing
                    await _database.InsertAllAsync(new[]
                    {
                        new Room { RoomNumber = 101, Availability = "Available", LocationId = mainBuildingId, Remark = "Standard room" },
                        new Room { RoomNumber = 102, Availability = "Available", LocationId = mainBuildingId, Remark = "Standard room" },
                        new Room { RoomNumber = 103, Availability = "Available", LocationId = mainBuildingId, Remark = "Deluxe room" },
                        new Room { RoomNumber = 201, Availability = "Available", LocationId = mainBuildingId, Remark = "Conference room" },
                        new Room { RoomNumber = 202, Availability = "Available", LocationId = mainBuildingId, Remark = "Meeting room" }
                    });
                }
            }
        }

        private static async Task EnsureDefaultUserAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            var existingUser = await _database.Table<User>()
                .Where(u => u.Role == UserRole.Admin)
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    FullName = "System Administrator",
                    Email = "admin@jindal.com",
                    Permissions = (int)Permission.All,
                    IsActive = true
                };

                await _database.InsertAsync(adminUser);
            }
        }


        // Room
        public static async Task<List<Room>> GetRooms()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
                
            var rooms = await _database.Table<Room>().ToListAsync();
            var locations = await GetLocations();

            foreach (var room in rooms)
            {
                room.LocationName = locations.FirstOrDefault(l => l.Id == room.LocationId)?.Name ?? "";
            }

            return rooms;
        }

        public static async Task AddRoom(Room room) 
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            await _database.InsertAsync(room);
        }
        
        public static async Task UpdateRoom(Room room) 
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            await _database.UpdateAsync(room);
        }
        
        public static async Task DeleteRoom(Room room) 
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            await _database.DeleteAsync(room);
        }

        public static async Task<List<Room>> GetAvailableRooms()
        {
            // Redirect to the new method with business logic
            return await GetAvailableRoomsWithLogic();
        }

        public static async Task<List<Room>> GetCompletelyAvailableRooms() => await GetAvailableRooms();

        // CheckInOut
        public static async Task<List<CheckInOut>> GetCheckInOuts()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            return await _database.Table<CheckInOut>().ToListAsync();
        }
        
        /// <summary>
        /// Get only active guests (those who haven't checked out yet)
        /// </summary>
        public static async Task<List<CheckInOut>> GetActiveGuests()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            return await _database.Table<CheckInOut>()
                .Where(c => c.CheckOutDate == null && c.CheckOutTime == null)
                .ToListAsync();
        }
        
        /// <summary>
        /// Get only checked out guests for reports
        /// </summary>
        public static async Task<List<CheckInOut>> GetCheckedOutGuests()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            return await _database.Table<CheckInOut>()
                .Where(c => c.CheckOutDate != null && c.CheckOutTime != null)
                .ToListAsync();
        }

        public static async Task<CheckInOut?> GetCheckInOutById(int id)
        {
            await Init();
            return await _database!.Table<CheckInOut>().FirstOrDefaultAsync(c => c.Id == id);
        }

        public static async Task<List<CheckInOut>> GetCheckInOutsByRoomNumber(int roomNumber)
        {
            await Init();
            return await _database!.Table<CheckInOut>()
                .Where(c => c.RoomNumber == roomNumber)
                .ToListAsync();
        }

        public static async Task AddCheckInOut(CheckInOut check) => await _database!.InsertAsync(check);
        public static async Task UpdateCheckInOut(CheckInOut check) => await _database!.UpdateAsync(check);
        public static async Task DeleteCheckInOut(CheckInOut check) => await _database!.DeleteAsync(check);

        // Location
        public static async Task<List<LocationModel>> GetLocations()
        {
            await Init();
            return await _database!.Table<LocationModel>().ToListAsync();
        }

        public static async Task<List<LocationModel>> SearchLocations(string query)
        {
            await Init();
            return await _database!.Table<LocationModel>()
                .Where(l => l.Name.Contains(query) || l.LocationCode.Contains(query))
                .ToListAsync();
        }

        public static async Task AddLocation(LocationModel loc) => await _database!.InsertAsync(loc);
        public static async Task UpdateLocation(LocationModel loc) => await _database!.UpdateAsync(loc);
        public static async Task DeleteLocation(LocationModel loc) => await _database!.DeleteAsync(loc);
        
        // Test database connectivity
        public static async Task<bool> TestDatabaseConnection()
        {
            try
            {
                await Init();
                // Try a simple query to test connectivity
                await _database!.Table<User>().CountAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // User Management Methods
        public static async Task<User?> AuthenticateUser(string username, string password)
        {
            try
            {
                await Init();
                
                var user = await _database!.Table<User>()
                    .Where(u => u.Username == username && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    user.LastLoginAt = DateTime.Now;
                    await _database.UpdateAsync(user);
                    return user;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Authentication error: {ex.Message}");
            }
            
            return null;
        }

        public static async Task<List<User>> GetAllUsers()
        {
            await Init();
            return await _database!.Table<User>().ToListAsync();
        }

        public static async Task<User?> GetUserById(int id)
        {
            await Init();
            return await _database!.Table<User>()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public static async Task<bool> CreateUser(User user)
        {
            try
            {
                await Init();
                
                // Hash password before saving
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                await _database!.InsertAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create user error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> UpdateUser(User user)
        {
            try
            {
                await Init();
                await _database!.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update user error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> DeleteUser(int userId)
        {
            try
            {
                await Init();
                var user = await GetUserById(userId);
                if (user != null)
                {
                    await _database!.DeleteAsync(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete user error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> ChangePassword(int userId, string oldPassword, string newPassword)
        {
            try
            {
                await Init();
                var user = await GetUserById(userId);
                if (user != null && BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    await _database!.UpdateAsync(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Change password error: {ex.Message}");
                return false;
            }
        }

        // Room Availability Management Methods
        public static async Task<List<Room>> GetAvailableRoomsWithLogic()
        {
            try
            {
                await Init();
                
                // Get all rooms
                var allRooms = await GetRooms();
                
                // Get active guests (not checked out)
                var activeGuests = await GetActiveGuests();
                
                // Get occupied room numbers
                var occupiedRoomNumbers = activeGuests
                    .Where(g => g.RoomNumber > 0)
                    .Select(g => g.RoomNumber)
                    .ToHashSet();
                
                // Filter available rooms
                var availableRooms = allRooms
                    .Where(r => !occupiedRoomNumbers.Contains(r.RoomNumber))
                    .ToList();
                
                // Update room availability status in database
                await UpdateRoomAvailabilityStatus(allRooms, occupiedRoomNumbers);
                
                return availableRooms;
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to get available rooms", ex, "DatabaseService");
                throw;
            }
        }

        public static async Task<List<(Room Room, List<CheckInOut> Guests)>> GetOccupiedRoomsWithGuests()
        {
            try
            {
                await Init();
                
                // Get all rooms
                var allRooms = await GetRooms();
                
                // Get active guests (not checked out)
                var activeGuests = await GetActiveGuests();
                
                // Group guests by room
                var guestsByRoom = activeGuests
                    .Where(g => g.RoomNumber > 0)
                    .GroupBy(g => g.RoomNumber)
                    .ToDictionary(g => g.Key, g => g.ToList());
                
                // Get occupied rooms with guests
                var occupiedRooms = new List<(Room Room, List<CheckInOut> Guests)>();
                
                foreach (var room in allRooms)
                {
                    if (guestsByRoom.TryGetValue(room.RoomNumber, out var guests))
                    {
                        occupiedRooms.Add((room, guests));
                    }
                }
                
                return occupiedRooms;
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to get occupied rooms with guests", ex, "DatabaseService");
                throw;
            }
        }

        public static async Task<bool> IsRoomAvailableByNumber(int roomNumber)
        {
            try
            {
                await Init();
                
                // Check if there are any active guests in this room
                var activeGuests = await GetActiveGuests();
                var hasActiveGuests = activeGuests.Any(g => g.RoomNumber == roomNumber);
                
                return !hasActiveGuests;
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError($"Failed to check room availability for room {roomNumber}", ex, "DatabaseService");
                throw;
            }
        }

        public static async Task<RoomUtilizationStats> GetRoomUtilizationStats()
        {
            try
            {
                await Init();
                
                var allRooms = await GetRooms();
                var activeGuests = await GetActiveGuests();
                
                var occupiedRoomNumbers = activeGuests
                    .Where(g => g.RoomNumber > 0)
                    .Select(g => g.RoomNumber)
                    .ToHashSet();
                
                var totalRooms = allRooms.Count;
                var occupiedRooms = allRooms.Count(r => occupiedRoomNumbers.Contains(r.RoomNumber));
                var availableRooms = totalRooms - occupiedRooms;
                var totalActiveGuests = activeGuests.Count;
                
                return new RoomUtilizationStats
                {
                    TotalRooms = totalRooms,
                    AvailableRooms = availableRooms,
                    OccupiedRooms = occupiedRooms,
                    TotalActiveGuests = totalActiveGuests,
                    UtilizationPercentage = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0
                };
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to get room utilization stats", ex, "DatabaseService");
                throw;
            }
        }

        public static async Task UpdateRoomAvailabilityStatus(List<Room>? allRooms = null, HashSet<int>? occupiedRoomNumbers = null)
        {
            try
            {
                await Init();
                
                // Get data if not provided
                if (allRooms == null)
                    allRooms = await GetRooms();
                
                if (occupiedRoomNumbers == null)
                {
                    var activeGuests = await GetActiveGuests();
                    occupiedRoomNumbers = activeGuests
                        .Where(g => g.RoomNumber > 0)
                        .Select(g => g.RoomNumber)
                        .ToHashSet();
                }
                
                // Update room availability status
                var roomsToUpdate = new List<Room>();
                
                foreach (var room in allRooms)
                {
                    var shouldBeOccupied = occupiedRoomNumbers.Contains(room.RoomNumber);
                    var currentlyAvailable = room.Availability == "Available";
                    
                    // Update if status doesn't match reality
                    if (shouldBeOccupied && currentlyAvailable)
                    {
                        room.Availability = "Booked";
                        roomsToUpdate.Add(room);
                    }
                    else if (!shouldBeOccupied && !currentlyAvailable)
                    {
                        room.Availability = "Available";
                        roomsToUpdate.Add(room);
                    }
                }
                
                // Update changed rooms in database
                if (roomsToUpdate.Any())
                {
                    await ErrorHandlingService.ExecuteWithRetry(async () =>
                    {
                        foreach (var room in roomsToUpdate)
                        {
                            await UpdateRoom(room);
                        }
                    }, 3, "Room Status Update");
                    
                    ErrorHandlingService.LogInfo($"Updated availability status for {roomsToUpdate.Count} rooms", "DatabaseService");
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError("Failed to update room availability status", ex, "DatabaseService");
                throw;
            }
        }

        // Get database statistics for debugging
        public static async Task<string> GetDatabaseStats()
        {
            try
            {
                await Init();
                var roomCount = await _database!.Table<Room>().CountAsync();
                var locationCount = await _database!.Table<LocationModel>().CountAsync();
                var guestCount = await _database!.Table<CheckInOut>().CountAsync();
                var userCount = await _database!.Table<User>().CountAsync();
                
                return $"Rooms: {roomCount}, Locations: {locationCount}, Guests: {guestCount}, Users: {userCount}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Represents room utilization statistics
    /// </summary>
    public class RoomUtilizationStats
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int TotalActiveGuests { get; set; }
        public double UtilizationPercentage { get; set; }
    }
}
