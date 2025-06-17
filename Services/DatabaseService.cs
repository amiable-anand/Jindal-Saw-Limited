using SQLite;
using Jindal.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Jindal.Services
{
    // This static service provides all database-related operations using SQLite
    public static class DatabaseService
    {
        static SQLiteAsyncConnection _database;

        /// <summary>
        /// Initializes the database connection and creates tables if they don't exist.
        /// Also seeds default data for Employee and Room tables.
        /// </summary>
        public static async Task Init()
        {
            if (_database != null)
                return;

            // Define the database path inside the app's local data directory
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Jindal.db");
            _database = new SQLiteAsyncConnection(databasePath);

            // Create tables for Employee, Room, and CheckInOut models
            await _database.CreateTableAsync<Employee>();
            await _database.CreateTableAsync<Room>();
            await _database.CreateTableAsync<CheckInOut>();

            // Seed default admin employee if none exists
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

            // Seed some default rooms if none are present
            var existingRooms = await _database.Table<Room>().ToListAsync();
            if (existingRooms.Count == 0)
            {
                var defaultRooms = new List<Room>
                {
                    new Room { RoomNumber = 101, Availability = "Available", Location = "First Floor", Remark = "", IsAvailable = true },
                    new Room { RoomNumber = 102, Availability = "Available", Location = "First Floor", Remark = "", IsAvailable = true },
                    new Room { RoomNumber = 201, Availability = "Available", Location = "Second Floor", Remark = "", IsAvailable = true },
                    new Room { RoomNumber = 202, Availability = "Available", Location = "Second Floor", Remark = "", IsAvailable = true }
                };

                await _database.InsertAllAsync(defaultRooms);
            }
        }

        /// <summary>
        /// Returns a matching employee based on credentials.
        /// Used for login authentication.
        /// </summary>
        public static async Task<Employee> GetEmployee(string username, string password)
        {
            await Init();
            return await _database.Table<Employee>()
                .FirstOrDefaultAsync(e => e.EmployeeCode == username && e.Password == password);
        }

        // ROOM OPERATIONS

        /// <summary>
        /// Returns all rooms from the database.
        /// </summary>
        public static async Task<List<Room>> GetRooms()
        {
            await Init();
            return await _database.Table<Room>().ToListAsync();
        }

        /// <summary>
        /// Adds a new room to the database.
        /// </summary>
        public static async Task AddRoom(Room room)
        {
            await Init();
            await _database.InsertAsync(room);
        }

        /// <summary>
        /// Updates the details of an existing room.
        /// </summary>
        public static async Task UpdateRoom(Room room)
        {
            await Init();
            await _database.UpdateAsync(room);
        }

        /// <summary>
        /// Deletes a room entry from the database.
        /// </summary>
        public static async Task DeleteRoom(Room room)
        {
            await Init();
            await _database.DeleteAsync(room);
        }

        // CHECK-IN/OUT OPERATIONS

        /// <summary>
        /// Returns all check-in/out records.
        /// </summary>
        public static async Task<List<CheckInOut>> GetCheckInOuts()
        {
            await Init();
            return await _database.Table<CheckInOut>().ToListAsync();
        }

        /// <summary>
        /// Adds a new check-in/out record.
        /// </summary>
        public static async Task AddCheckInOut(CheckInOut check)
        {
            await Init();
            await _database.InsertAsync(check);
        }

        /// <summary>
        /// Updates an existing check-in/out record.
        /// </summary>
        public static async Task UpdateCheckInOut(CheckInOut check)
        {
            await Init();
            await _database.UpdateAsync(check);
        }

        /// <summary>
        /// Deletes a check-in/out record.
        /// </summary>
        public static async Task DeleteCheckInOut(CheckInOut check)
        {
            await Init();
            await _database.DeleteAsync(check);
        }

        /// <summary>
        /// Retrieves all available rooms (IsAvailable = true).
        /// Includes a temporary fix to mark all rooms available.
        /// </summary>
        public static async Task<List<Room>> GetAvailableRooms()
        {
            await Init();

            // Fetch all rooms first
            var allRooms = await _database.Table<Room>().ToListAsync();
            System.Diagnostics.Debug.WriteLine($"All Rooms Count: {allRooms.Count}");

            // ⚠ TEMPORARY: Mark all rooms as available
            foreach (var room in allRooms)
            {
                room.IsAvailable = true;
                await _database.UpdateAsync(room);
            }

            // Filter rooms where IsAvailable is true
            var availableRooms = allRooms.Where(r => r.IsAvailable == true).ToList();
            System.Diagnostics.Debug.WriteLine($"Available Rooms Count: {availableRooms.Count}");

            return availableRooms;
        }

    }
}
