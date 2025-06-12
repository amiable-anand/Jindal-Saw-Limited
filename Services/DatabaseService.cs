using SQLite;
using Jindal.Models;
using System.IO;

namespace Jindal.Services
{
    public class DatabaseService
    {
        private static SQLiteAsyncConnection _database;

        public static async Task Init()
        {
            if (_database != null)
                return;

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "employees.db");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<Employee>();
        }

        public static async Task<Employee> GetEmployee(string code, string password)
        {
            await Init();
            return await _database.Table<Employee>()
                .Where(emp => emp.EmployeeCode == code && emp.Password == password)
                .FirstOrDefaultAsync();
        }

        // Optional: Seed a test employee
        public static async Task AddTestEmployee()
        {
            await Init();
            var existing = await _database.Table<Employee>().Where(e => e.EmployeeCode == "E123").FirstOrDefaultAsync();
            if (existing == null)
            {
                await _database.InsertAsync(new Employee
                {
                    EmployeeCode = "E123",
                    Password = "pass@123"
                });
            }
        }
    }
}
