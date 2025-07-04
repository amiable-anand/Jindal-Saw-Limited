using SQLite;
using Jindal.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Storage;
using System.Linq;
using LocationModel = Jindal.Models.Location;

namespace Jindal.Services
{
    public static class DatabaseService
    {
        static SQLiteAsyncConnection _database;

        public static async Task Init()
        {
            if (_database != null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Jindal.db");
            _database = new SQLiteAsyncConnection(databasePath);

            await _database.CreateTableAsync<Employee>();
            await _database.CreateTableAsync<Room>();
            await _database.CreateTableAsync<CheckInOut>();
            await _database.CreateTableAsync<LocationModel>();

            // Ensure default admin
            var existing = await _database.Table<Employee>().FirstOrDefaultAsync();
            if (existing == null)
            {
                await _database.InsertAsync(new Employee
                {
                    EmployeeCode = "admin",
                    Password = "admin123"
                });
            }

            // Ensure default locations exist
            var locations = await GetLocations();
            if (!locations.Any())
            {
                var defaultLocations = new List<LocationModel>
                {
                    new() { Name = "First Floor", LocationCode = "FF", Address = "", Remark = "" },
                    new() { Name = "Second Floor", LocationCode = "SF", Address = "", Remark = "" }
                };

                await _database.InsertAllAsync(defaultLocations);
                locations = await GetLocations(); // refresh
            }

            // Insert demo rooms if empty
            var existingRooms = await _database.Table<Room>().ToListAsync();
            if (!existingRooms.Any())
            {
                var ff = locations.FirstOrDefault(l => l.Name == "First Floor")?.Id ?? 0;
                var sf = locations.FirstOrDefault(l => l.Name == "Second Floor")?.Id ?? 0;

                await _database.InsertAllAsync(new List<Room>
                {
                    new Room { RoomNumber = 101, Availability = "Available", LocationId = ff, Remark = "" },
                    new Room { RoomNumber = 102, Availability = "Available", LocationId = ff, Remark = "" },
                    new Room { RoomNumber = 201, Availability = "Available", LocationId = sf, Remark = "" },
                    new Room { RoomNumber = 202, Availability = "Available", LocationId = sf, Remark = "" }
                });
            }

            // 🚨 Optional: Migrate old string-based Location to LocationId (if old data exists)
            // NOTE: Only include if upgrading an existing database
            /*
            var allLocations = await GetLocations();
            var roomsWithOldLocation = await _database.Table<Room>()
                .Where(r => r.Location != null && r.Location != "")
                .ToListAsync();

            foreach (var room in roomsWithOldLocation)
            {
                var match = allLocations.FirstOrDefault(l => l.Name == room.Location);
                if (match != null)
                {
                    room.LocationId = match.Id;
                    room.Location = null;
                    await _database.UpdateAsync(room);
                }
            }
            */
        }

        public static async Task<Employee> GetEmployee(string username, string password)
        {
            await Init();
            return await _database.Table<Employee>()
                .FirstOrDefaultAsync(e => e.EmployeeCode == username && e.Password == password);
        }

        public static async Task<List<Room>> GetRooms()
        {
            await Init();

            var rooms = await _database.Table<Room>().ToListAsync();
            var locations = await GetLocations();

            // Add location name for display
            foreach (var room in rooms)
            {
                room.LocationName = locations.FirstOrDefault(l => l.Id == room.LocationId)?.Name ?? "";
            }

            return rooms;
        }

        public static async Task AddRoom(Room room)
        {
            await Init();
            await _database.InsertAsync(room);
        }

        public static async Task UpdateRoom(Room room)
        {
            await Init();
            await _database.UpdateAsync(room);
        }

        public static async Task DeleteRoom(Room room)
        {
            await Init();
            await _database.DeleteAsync(room);
        }

        public static async Task<List<Room>> GetAvailableRooms()
        {
            await Init();

            var occupiedRoomNumbers = (await _database.Table<CheckInOut>()
                .Where(c => c.CheckOutDate == null && c.CheckOutTime == null)
                .ToListAsync())
                .Select(c => c.RoomNumber)
                .Distinct()
                .ToHashSet();

            var rooms = await _database.Table<Room>()
                .Where(r => !occupiedRoomNumbers.Contains(r.RoomNumber.ToString()))
                .ToListAsync();

            var locations = await GetLocations();
            foreach (var room in rooms)
            {
                room.LocationName = locations.FirstOrDefault(l => l.Id == room.LocationId)?.Name ?? "";
            }

            return rooms;
        }

        public static async Task<List<Room>> GetCompletelyAvailableRooms()
        {
            await Init();

            var allRooms = await _database.Table<Room>().ToListAsync();
            var allCheckIns = await _database.Table<CheckInOut>().ToListAsync();

            var occupiedRoomNumbers = allCheckIns
                .Where(c => c.CheckOutDate == null && c.CheckOutTime == null)
                .Select(c => c.RoomNumber)
                .Distinct()
                .ToHashSet();

            var availableRooms = allRooms
                .Where(r => !occupiedRoomNumbers.Contains(r.RoomNumber.ToString()))
                .ToList();

            var locations = await GetLocations();
            foreach (var room in availableRooms)
            {
                room.LocationName = locations.FirstOrDefault(l => l.Id == room.LocationId)?.Name ?? "";
            }

            return availableRooms;
        }

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

        public static async Task AddCheckInOut(CheckInOut check)
        {
            await Init();
            await _database.InsertAsync(check);
        }

        public static async Task UpdateCheckInOut(CheckInOut check)
        {
            await Init();
            await _database.UpdateAsync(check);
        }

        public static async Task DeleteCheckInOut(CheckInOut check)
        {
            await Init();
            await _database.DeleteAsync(check);
        }

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

        public static async Task AddLocation(LocationModel loc)
        {
            await Init();
            await _database.InsertAsync(loc);
        }

        public static async Task UpdateLocation(LocationModel loc)
        {
            await Init();
            await _database.UpdateAsync(loc);
        }

        public static async Task DeleteLocation(LocationModel loc)
        {
            await Init();
            await _database.DeleteAsync(loc);
        }
    }
}
