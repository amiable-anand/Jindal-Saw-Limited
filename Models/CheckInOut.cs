using System;
using SQLite;

namespace Jindal.Models
{
    public class CheckInOut
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Main Guest and Room Info
        public int RoomNumber { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestIdNumber { get; set; } = string.Empty;
        public string IdType { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;

        // Check-in / Check-out Details
        public DateTime CheckInDate { get; set; } = DateTime.Now;
        public TimeSpan CheckInTime { get; set; } = DateTime.Now.TimeOfDay;
        public DateTime? CheckOutDate { get; set; }
        public TimeSpan? CheckOutTime { get; set; }

        // Department, Purpose, Mail info
        public string Department { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public DateTime MailReceivedDate { get; set; } = DateTime.Now;
    }
}
