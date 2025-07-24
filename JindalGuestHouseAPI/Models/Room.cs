using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JindalGuestHouseAPI.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int RoomNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string Availability { get; set; } = "Available";

        [Required]
        public int LocationId { get; set; }

        [StringLength(1000)]
        [Column(TypeName = "nvarchar(1000)")]
        public string Remark { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }

        public virtual ICollection<CheckInOut> CheckInOuts { get; set; } = new List<CheckInOut>();

        // Computed properties
        [NotMapped]
        public bool IsAvailable
        {
            get => Availability == "Available";
            set => Availability = value ? "Available" : "Booked";
        }

        [NotMapped]
        public string LocationName { get; set; } = string.Empty;
    }
}
