using SQLite;
namespace Jindal.Models;


public class Room
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int RoomNumber { get; set; }

    public string Availability { get; set; }

    public string Location { get; set; }

    public string Remark { get; set; }

    public bool IsAvailable
    {
        get => Availability == "Available";
        set => Availability = value ? "Available" : "Booked";
    }

    public override string ToString() => RoomNumber.ToString();
}
