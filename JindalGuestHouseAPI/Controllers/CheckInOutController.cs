using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JindalGuestHouseAPI.Data;
using JindalGuestHouseAPI.Models;

namespace JindalGuestHouseAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CheckInOutController : ControllerBase
    {
        private readonly GuestHouseDbContext _context;
        private readonly ILogger<CheckInOutController> _logger;

        public CheckInOutController(GuestHouseDbContext context, ILogger<CheckInOutController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/CheckInOut
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCheckInOuts()
        {
            var checkInOuts = await _context.CheckInOuts
                .Include(c => c.Room)
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.GuestName,
                    c.GuestIdNumber,
                    c.IdType,
                    c.CheckInDate,
                    c.CheckInTime,
                    c.CheckOutDate,
                    c.CheckOutTime,
                    Status = c.IsCheckedOut ? "Checked Out" : "Checked In",
                    c.Room.RoomNumber,
                    LocationName = c.Room.Location.Name
                })
                .ToListAsync();

            return Ok(checkInOuts);
        }

        // GET: api/CheckInOut/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCheckInOut(int id)
        {
            var checkInOut = await _context.CheckInOuts
                .Include(c => c.Room)
                .Where(c => c.Id == id && c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.GuestName,
                    c.GuestIdNumber,
                    c.IdType,
                    c.CompanyName,
                    c.Nationality,
                    c.Address,
                    c.Mobile,
                    c.CheckInDate,
                    c.CheckInTime,
                    c.CheckOutDate,
                    c.CheckOutTime,
                    c.Department,
                    c.Purpose,
                    Status = c.IsCheckedOut ? "Checked Out" : "Checked In",
                    c.Room.RoomNumber,
                    LocationName = c.Room.Location.Name
                })
                .FirstOrDefaultAsync();

            if (checkInOut == null)
            {
                return NotFound(new { message = "CheckInOut not found" });
            }

            return Ok(checkInOut);
        }

        // POST: api/CheckInOut
        [HttpPost]
        public async Task<ActionResult<CheckInOut>> CreateCheckInOut(CreateCheckInOutRequest request)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(request.RoomId);
                if (room == null || !room.IsActive)
                {
                    return BadRequest(new { message = "Room not found or inactive" });
                }

                var checkInOut = new CheckInOut
                {
                    RoomNumber = room.RoomNumber,
                    GuestName = request.GuestName,
                    GuestIdNumber = request.GuestIdNumber,
                    IdType = request.IdType,
                    CompanyName = request.CompanyName,
                    Nationality = request.Nationality,
                    Address = request.Address,
                    Mobile = request.Mobile,
                    CheckInDate = request.CheckInDate,
                    CheckInTime = request.CheckInTime,
                    Department = request.Department,
                    Purpose = request.Purpose,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.CheckInOuts.Add(checkInOut);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCheckInOut), new { id = checkInOut.Id }, checkInOut);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating check-in/out record");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT: api/CheckInOut/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCheckInOut(int id, UpdateCheckInOutRequest request)
        {
            try
            {
                var checkInOut = await _context.CheckInOuts.FindAsync(id);
                if (checkInOut == null || !checkInOut.IsActive)
                {
                    return NotFound(new { message = "CheckInOut record not found or inactive" });
                }

                checkInOut.GuestName = request.GuestName ?? checkInOut.GuestName;
                checkInOut.GuestIdNumber = request.GuestIdNumber ?? checkInOut.GuestIdNumber;
                checkInOut.IdType = request.IdType ?? checkInOut.IdType;
                checkInOut.CompanyName = request.CompanyName ?? checkInOut.CompanyName;
                checkInOut.Nationality = request.Nationality ?? checkInOut.Nationality;
                checkInOut.Address = request.Address ?? checkInOut.Address;
                checkInOut.Mobile = request.Mobile ?? checkInOut.Mobile;
                checkInOut.CheckInDate = request.CheckInDate ?? checkInOut.CheckInDate;
                checkInOut.CheckInTime = request.CheckInTime ?? checkInOut.CheckInTime;
                checkInOut.CheckOutDate = request.CheckOutDate;
                checkInOut.CheckOutTime = request.CheckOutTime;
                checkInOut.Department = request.Department ?? checkInOut.Department;
                checkInOut.Purpose = request.Purpose ?? checkInOut.Purpose;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating check-in/out record {CheckInOutId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE: api/CheckInOut/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCheckInOut(int id)
        {
            try
            {
                var checkInOut = await _context.CheckInOuts.FindAsync(id);
                if (checkInOut == null || !checkInOut.IsActive)
                {
                    return NotFound(new { message = "CheckInOut record not found or inactive" });
                }

                // Soft delete
                checkInOut.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "CheckInOut record deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting check-in/out record {CheckInOutId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    // Request DTOs
    public class CreateCheckInOutRequest
    {
        public int RoomId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestIdNumber { get; set; } = string.Empty;
        public string IdType { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Nationality { get; set; }
        public string? Address { get; set; }
        public string? Mobile { get; set; }
        public DateTime CheckInDate { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public string? Department { get; set; }
        public string? Purpose { get; set; }
    }

    public class UpdateCheckInOutRequest
    {
        public string? GuestName { get; set; }
        public string? GuestIdNumber { get; set; }
        public string? IdType { get; set; }
        public string? CompanyName { get; set; }
        public string? Nationality { get; set; }
        public string? Address { get; set; }
        public string? Mobile { get; set; }
        public DateTime? CheckInDate { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? Department { get; set; }
        public string? Purpose { get; set; }
    }
}

