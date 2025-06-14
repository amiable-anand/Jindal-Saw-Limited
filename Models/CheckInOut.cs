using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite;

namespace Jindal.Models
{
    public class CheckInOut
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int RoomNumber { get; set; }
        public string GuestName { get; set; }
        public string GuestIdNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string Department { get; set; }
        public string Purpose { get; set; }
        public DateTime MailReceivedDate { get; set; }
    }
}
