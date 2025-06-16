using SQLite;

public class CheckInOut
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string RoomNumber { get; set; }
    public string GuestName { get; set; }
    public string GuestIdNumber { get; set; }
    public string IdType { get; set; }            // ✅ Add this
    public string CompanyName { get; set; }       // ✅ Add this
    public string Nationality { get; set; }       // ✅ Add this
    public string Address { get; set; }           // ✅ Add this
    public string Mobile { get; set; }            // ✅ Add this
    public DateTime CheckInDate { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string Department { get; set; }
    public string Purpose { get; set; }
    public DateTime MailReceivedDate { get; set; }
}