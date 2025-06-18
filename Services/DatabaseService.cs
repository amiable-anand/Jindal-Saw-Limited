using SQLite;
using Jindal.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Storage;
using System.Linq;

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

            var existing = await _database.Table<Employee>().FirstOrDefaultAsync();
            if (existing == null)
            {
                var defaultEmployee = new Employee
                {
                    EmployeeCode = "admin",
                    Password = "admin123"
                };
                await _database.InsertAsync(defaultEmployee);
            }

            var existingRooms = await _database.Table<Room>().ToListAsync();
            if (existingRooms.Count == 0)
            {
                var defaultRooms = new List<Room>
                {
                    new Room { RoomNumber = 101, Availability = "Available", Location = "First Floor", Remark = "" },
                    new Room { RoomNumber = 102, Availability = "Available", Location = "First Floor", Remark = "" },
                    new Room { RoomNumber = 201, Availability = "Available", Location = "Second Floor", Remark = "" },
                    new Room { RoomNumber = 202, Availability = "Available", Location = "Second Floor", Remark = "" }
                };

                await _database.InsertAllAsync(defaultRooms);
            }
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
            return await _database.Table<Room>().ToListAsync();
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

        public static async Task<List<CheckInOut>> GetCheckInOuts()
        {
            await Init();
            return await _database.Table<CheckInOut>().ToListAsync();
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

        public static async Task<List<Room>> GetAvailableRooms()
        {
            await Init();

            var availableRooms = await _database.Table<Room>()
                .Where(r => r.Availability == "Available")
                .ToListAsync();

            System.Diagnostics.Debug.WriteLine($"Available Rooms Count: {availableRooms.Count}");
            return availableRooms;
        }
    }
}
