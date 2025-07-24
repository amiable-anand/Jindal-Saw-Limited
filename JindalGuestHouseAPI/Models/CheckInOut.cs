using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JindalGuestHouseAPI.Models
{
    public class CheckInOut
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Main Guest and Room Info
        [Required]
        public int RoomNumber { get; set; }

        [Required]
        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string GuestIdNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string IdType { get; set; } = string.Empty;

        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string Nationality { get; set; } = string.Empty;

        [StringLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        [Column(TypeName = "nvarchar(20)")]
        public string Mobile { get; set; } = string.Empty;

        // Check-in / Check-out Details
        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CheckInDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "time")]
        public TimeSpan CheckInTime { get; set; } = DateTime.Now.TimeOfDay;

        [Column(TypeName = "datetime2")]
        public DateTime? CheckOutDate { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan? CheckOutTime { get; set; }

        // Department, Purpose, Mail info
        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Department { get; set; } = string.Empty;

        [StringLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        public string Purpose { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime MailReceivedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Room? Room { get; set; }

        // Computed properties
        [NotMapped]
        public bool IsCheckedOut => CheckOutDate.HasValue && CheckOutTime.HasValue;

        [NotMapped]
        public string Status => IsCheckedOut ? "Checked Out" : "Checked In";

        [NotMapped]
        public TimeSpan? StayDuration
        {
            get
            {
                if (!IsCheckedOut) return null;
                
                var checkIn = CheckInDate.Add(CheckInTime);
                var checkOut = CheckOutDate!.Value.Add(CheckOutTime!.Value);
                return checkOut - checkIn;
            }
        }
    }
}
