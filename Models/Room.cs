using SQLite;

namespace Jindal.Models
{
    public class Room
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int RoomNumber { get; set; }

        public string Availability { get; set; }

        public int LocationId { get; set; } // 🔄 Replaces old string Location

        public string Remark { get; set; }

        // For runtime display only
        [Ignore]
        public string LocationName { get; set; } // fetched via join in code

        public bool IsAvailable
        {
            get => Availability == "Available";
            set => Availability = value ? "Available" : "Booked";
        }

        public override string ToString() => RoomNumber.ToString();
    }
}
