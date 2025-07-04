using SQLite;
using Jindal.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LocationModel = Jindal.Models.Location;

namespace Jindal.Services
{
    /// <summary>
    /// Service class to manage all database operations using SQLite.
    /// </summary>
    public static class DatabaseService
    {
        private static SQLiteAsyncConnection _database;

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
            var existing = await _database.Table<Employee>().FirstOrDefaultAsync();
            if (existing == null)
            {
                await _database.InsertAsync(new Employee
                {
                    EmployeeCode = "admin",
                    Password = "admin123"
                });
            }
        }

        private static async Task EnsureDefaultLocationsAsync()
        {
            var locations = await GetLocations();
            if (!locations.Any())
            {
                var defaults = new List<LocationModel>
                {
                    new() { Name = "First Floor", LocationCode = "FF" },
                    new() { Name = "Second Floor", LocationCode = "SF" }
                };

                await _database.InsertAllAsync(defaults);
            }
        }

        private static async Task EnsureDemoRoomsAsync()
        {
            var rooms = await _database.Table<Room>().ToListAsync();
            if (rooms.Any()) return;

            var locations = await GetLocations();
            var ff = locations.FirstOrDefault(l => l.Name == "First Floor")?.Id ?? 0;
            var sf = locations.FirstOrDefault(l => l.Name == "Second Floor")?.Id ?? 0;

            var demoRooms = new List<Room>
            {
                new() { RoomNumber = 101, Availability = "Available", LocationId = ff },
                new() { RoomNumber = 102, Availability = "Available", LocationId = ff },
                new() { RoomNumber = 201, Availability = "Available", LocationId = sf },
                new() { RoomNumber = 202, Availability = "Available", LocationId = sf }
            };

            await _database.InsertAllAsync(demoRooms);
        }

        // Employee
        public static async Task<Employee> GetEmployee(string username, string password)
        {
            await Init();
            return await _database.Table<Employee>()
                .FirstOrDefaultAsync(e => e.EmployeeCode == username && e.Password == password);
        }

        // Room
        public static async Task<List<Room>> GetRooms()
        {
            await Init();
            var rooms = await _database.Table<Room>().ToListAsync();
            var locations = await GetLocations();

            foreach (var room in rooms)
            {
                room.LocationName = locations.FirstOrDefault(l => l.Id == room.LocationId)?.Name ?? "";
            }

            return rooms;
        }

        public static async Task AddRoom(Room room) => await _database.InsertAsync(room);
        public static async Task UpdateRoom(Room room) => await _database.UpdateAsync(room);
        public static async Task DeleteRoom(Room room) => await _database.DeleteAsync(room);

        public static async Task<List<Room>> GetAvailableRooms()
        {
            await Init();

            var occupiedRoomNumbers = (await _database.Table<CheckInOut>()
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
            return await _database.Table<CheckInOut>().ToListAsync();
        }

        public static async Task<CheckInOut> GetCheckInOutById(int id)
        {
            await Init();
            return await _database.Table<CheckInOut>().FirstOrDefaultAsync(c => c.Id == id);
        }

        public static async Task<List<CheckInOut>> GetCheckInOutsByRoomNumber(string roomNumber)
        {
            await Init();
            return await _database.Table<CheckInOut>()
                .Where(c => c.RoomNumber == roomNumber)
                .ToListAsync();
        }

        public static async Task AddCheckInOut(CheckInOut check) => await _database.InsertAsync(check);
        public static async Task UpdateCheckInOut(CheckInOut check) => await _database.UpdateAsync(check);
        public static async Task DeleteCheckInOut(CheckInOut check) => await _database.DeleteAsync(check);

        // Location
        public static async Task<List<LocationModel>> GetLocations()
        {
            await Init();
            return await _database.Table<LocationModel>().ToListAsync();
        }

        public static async Task<List<LocationModel>> SearchLocations(string query)
        {
            await Init();
            return await _database.Table<LocationModel>()
                .Where(l => l.Name.Contains(query) || l.LocationCode.Contains(query))
                .ToListAsync();
        }

        public static async Task AddLocation(LocationModel loc) => await _database.InsertAsync(loc);
        public static async Task UpdateLocation(LocationModel loc) => await _database.UpdateAsync(loc);
        public static async Task DeleteLocation(LocationModel loc) => await _database.DeleteAsync(loc);
    }
}
