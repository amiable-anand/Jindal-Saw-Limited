using SQLite;
using Jindal.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

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


            // 🔐 Seed default employee if not exists
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

    }
}
