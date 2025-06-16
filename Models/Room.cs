namespace Jindal.Models
{
    public class Room
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }

        public int RoomNumber { get; set; }
        public string Availability { get; set; }
        public string Location { get; set; }
        public string Remark { get; set; }
            public bool IsAvailable { get; set; }
    }
}
