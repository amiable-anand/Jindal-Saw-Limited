using SQLite;

namespace Jindal.Models
{
    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, NotNull]
        public string EmployeeCode { get; set; } = string.Empty;

        [NotNull]
        public string Password { get; set; } = string.Empty;
    }
}
