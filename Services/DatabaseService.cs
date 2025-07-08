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

            await _database.CreateTableAsync<Employee>();
            await _database.CreateTableAsync<Room>();
            await _database.CreateTableAsync<CheckInOut>();
            await _database.CreateTableAsync<LocationModel>();

            await EnsureDefaultAdminAsync();
            await EnsureDefaultLocationsAsync();
            await EnsureDemoRoomsAsync();
        }

        private static async Task EnsureDefaultAdminAsync()
        {
            var existing = await _database!.Table<Employee>().FirstOrDefaultAsync();
            if (existing == null)
            {
                // Create secure default admin with hashed password
                // Default password is "JindalAdmin2024!" - change this after first login
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword("JindalAdmin2024!");
                await _database!.InsertAsync(new Employee
                {
                    EmployeeCode = "admin",
                    Password = hashedPassword
                });
            }
        }

        private static async Task EnsureDefaultLocationsAsync()
        {
            var existingLocations = await _database!.Table<LocationModel>().CountAsync();
            if (existingLocations == 0)
            {
                // Add some demo locations for testing
                await _database!.InsertAllAsync(new[]
                {
                    new LocationModel { Name = "Main Building", LocationCode = "MB", Address = "123 Main St" },
                    new LocationModel { Name = "Guest House", LocationCode = "GH", Address = "456 Guest Ave" },
                    new LocationModel { Name = "Conference Center", LocationCode = "CC", Address = "789 Conference Blvd" }
                });
            }
        }

        private static async Task EnsureDemoRoomsAsync()
        {
            var existingRooms = await _database!.Table<Room>().CountAsync();
            if (existingRooms == 0)
            {
                // Ensure we have locations first
                var locations = await _database!.Table<LocationModel>().ToListAsync();
                if (locations.Any())
                {
                    var mainBuildingId = locations.First().Id;
                    
                    // Add some demo rooms for testing
                    await _database!.InsertAllAsync(new[]
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

        // Employee
        public static async Task<Employee?> GetEmployee(string username, string password)
        {
            await Init();
            var employee = await _database!.Table<Employee>()
                .FirstOrDefaultAsync(e => e.EmployeeCode == username);
            
            if (employee != null && BCrypt.Net.BCrypt.Verify(password, employee.Password))
            {
                return employee;
            }
            
            return null;
        }

        // Room
        public static async Task<List<Room>> GetRooms()
        {
            await Init();
            var rooms = await _database!.Table<Room>().ToListAsync();
            var locations = await GetLocations();

            foreach (var room in rooms)
            {
                room.LocationName = locations.FirstOrDefault(l => l.Id == room.LocationId)?.Name ?? "";
            }

            return rooms;
        }

        public static async Task AddRoom(Room room) => await _database!.InsertAsync(room);
        public static async Task UpdateRoom(Room room) => await _database!.UpdateAsync(room);
        public static async Task DeleteRoom(Room room) => await _database!.DeleteAsync(room);

        public static async Task<List<Room>> GetAvailableRooms()
        {
            await Init();

            var occupiedRoomNumbers = (await _database!.Table<CheckInOut>()
                .Where(c => c.CheckOutDate == null && c.CheckOutTime == null)
                .ToListAsync())
                .Where(c => !string.IsNullOrWhiteSpace(c.RoomNumber))
                .Select(c => c.RoomNumber.Trim())
                .ToHashSet();

            var allRooms = await _database.Table<Room>().ToListAsync();

            var available = allRooms
                .Where(r => !occupiedRoomNumbers.Contains(r.RoomNumber.ToString()))
                .ToList();

            var locations = await GetLocations();
            foreach (var room in available)
            {
                room.LocationName = locations.FirstOrDefault(l => l.Id == room.LocationId)?.Name ?? "";
            }

            return available;
        }

        public static async Task<List<Room>> GetCompletelyAvailableRooms() => await GetAvailableRooms();

        // CheckInOut
        public static async Task<List<CheckInOut>> GetCheckInOuts()
        {
            await Init();
            return await _database!.Table<CheckInOut>().ToListAsync();
        }
        
        /// <summary>
        /// Get only active guests (those who haven't checked out yet)
        /// </summary>
        public static async Task<List<CheckInOut>> GetActiveGuests()
        {
            await Init();
            return await _database!.Table<CheckInOut>()
                .Where(c => c.CheckOutDate == null && c.CheckOutTime == null)
                .ToListAsync();
        }
        
        /// <summary>
        /// Get only checked out guests for reports
        /// </summary>
        public static async Task<List<CheckInOut>> GetCheckedOutGuests()
        {
            await Init();
            return await _database!.Table<CheckInOut>()
                .Where(c => c.CheckOutDate != null && c.CheckOutTime != null)
                .ToListAsync();
        }

        public static async Task<CheckInOut?> GetCheckInOutById(int id)
        {
            await Init();
            return await _database!.Table<CheckInOut>().FirstOrDefaultAsync(c => c.Id == id);
        }

        public static async Task<List<CheckInOut>> GetCheckInOutsByRoomNumber(string roomNumber)
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
                await _database!.Table<Employee>().CountAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // Get database statistics for debugging
        public static async Task<string> GetDatabaseStats()
        {
            try
            {
                await Init();
                var employeeCount = await _database!.Table<Employee>().CountAsync();
                var roomCount = await _database!.Table<Room>().CountAsync();
                var locationCount = await _database!.Table<LocationModel>().CountAsync();
                var guestCount = await _database!.Table<CheckInOut>().CountAsync();
                
                return $"Employees: {employeeCount}, Rooms: {roomCount}, Locations: {locationCount}, Guests: {guestCount}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
