using Microsoft.EntityFrameworkCore;
using JindalGuestHouseAPI.Models;
using ApiLocation = JindalGuestHouseAPI.Models.Location;

namespace JindalGuestHouseAPI.Data
{
    public class GuestHouseDbContext : DbContext
    {
        public GuestHouseDbContext(DbContextOptions<GuestHouseDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ApiLocation> Locations { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<CheckInOut> CheckInOuts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FullName).HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email);
            });

            // Configure Location entity
            modelBuilder.Entity<ApiLocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.LocationCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Remark).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.LocationCode).IsUnique();
                entity.HasIndex(e => e.Name);
            });

            // Configure Room entity
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoomNumber).IsRequired();
                entity.Property(e => e.Availability).IsRequired().HasMaxLength(50).HasDefaultValue("Available");
                entity.Property(e => e.LocationId).IsRequired();
                entity.Property(e => e.Remark).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                
                // Foreign key relationship
                entity.HasOne(r => r.Location)
                      .WithMany(l => l.Rooms)
                      .HasForeignKey(r => r.LocationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.RoomNumber).IsUnique();
                entity.HasIndex(e => e.LocationId);
            });

            // Configure CheckInOut entity
            modelBuilder.Entity<CheckInOut>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoomNumber).IsRequired();
                entity.Property(e => e.GuestName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.GuestIdNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IdType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.Nationality).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Mobile).HasMaxLength(20);
                entity.Property(e => e.Department).HasMaxLength(200);
                entity.Property(e => e.Purpose).HasMaxLength(500);
                entity.Property(e => e.CheckInDate).IsRequired();
                entity.Property(e => e.CheckInTime).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                // Foreign key relationship with Room
                entity.HasOne(c => c.Room)
                      .WithMany(r => r.CheckInOuts)
                      .HasForeignKey(c => c.RoomNumber)
                      .HasPrincipalKey(r => r.RoomNumber)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.RoomNumber);
                entity.HasIndex(e => e.GuestIdNumber);
                entity.HasIndex(e => e.CheckInDate);
                entity.HasIndex(e => e.CheckOutDate);
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is CheckInOut && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is CheckInOut checkInOut)
                {
                    checkInOut.UpdatedAt = DateTime.Now;
                }
            }
        }
    }
}
