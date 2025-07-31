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
public class RoomsController : ControllerBase
    {
        private readonly GuestHouseDbContext _context;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(GuestHouseDbContext context, ILogger<RoomsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRooms()
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsActive)
                .Include(r => r.Location)
                .Select(r => new
                {
                    r.Id,
                    r.RoomNumber,
                    r.Availability,
                    r.Remark,
                    r.CreatedAt,
                    r.IsActive,
                    LocationName = r.Location!.Name
                })
                .ToListAsync();

            return Ok(rooms);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Location)
                .Where(r => r.Id == id && r.IsActive)
                .Select(r => new
                {
                    r.Id,
                    r.RoomNumber,
                    r.Availability,
                    r.Remark,
                    r.CreatedAt,
                    r.IsActive,
                    LocationName = r.Location!.Name
                })
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return NotFound(new { message = "Room not found" });
            }

            return Ok(room);
        }

        // GET: api/Rooms/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableRooms()
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsActive && r.Availability == "Available")
                .Include(r => r.Location)
                .Select(r => new
                {
                    r.Id,
                    r.RoomNumber,
                    r.Availability,
                    r.Remark,
                    r.CreatedAt,
                    r.IsActive,
                    LocationName = r.Location!.Name
                })
                .ToListAsync();

            return Ok(rooms);
        }

        // POST: api/Rooms
        [HttpPost]
        public async Task<ActionResult<Room>> CreateRoom(CreateRoomRequest request)
        {
            try
            {
                // Check if room number already exists
                if (await _context.Rooms.AnyAsync(r => r.RoomNumber == request.RoomNumber && r.LocationId == request.LocationId))
                {
                    return BadRequest(new { message = "Room number already exists in this location" });
                }

                var room = new Room
                {
                    RoomNumber = request.RoomNumber,
                    Availability = request.Availability ?? "Available",
                    LocationId = request.LocationId,
                    Remark = request.Remark ?? string.Empty,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, UpdateRoomRequest request)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    return NotFound(new { message = "Room not found" });
                }

                // Check if room number already exists in a location
                if ((request.RoomNumber != room.RoomNumber || request.LocationId != room.LocationId) &&
                    await _context.Rooms.AnyAsync(r => r.RoomNumber == request.RoomNumber && r.LocationId == request.LocationId && r.Id != id))
                {
                    return BadRequest(new { message = "Room number already exists in this location" });
                }

                room.RoomNumber = request.RoomNumber;
                room.Availability = request.Availability ?? room.Availability;
                room.LocationId = request.LocationId;
                room.Remark = request.Remark ?? room.Remark;
                room.IsActive = request.IsActive;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {RoomId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    return NotFound(new { message = "Room not found" });
                }

                // Soft delete
                room.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Room deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room {RoomId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET: api/Rooms/5/checkinouts
        [HttpGet("{id}/checkinouts")]
        public async Task<ActionResult<IEnumerable<object>>> GetRoomCheckInOuts(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound(new { message = "Room not found" });
            }

            var checkInOuts = await _context.CheckInOuts
                .Where(c => c.RoomNumber == room.RoomNumber)
                .Select(c => new
                {
                    c.Id,
                    c.GuestName,
                    c.GuestIdNumber,
                    c.CheckInDate,
                    c.CheckInTime,
                    c.CheckOutDate,
                    c.CheckOutTime,
                    Status = c.IsCheckedOut ? "Checked Out" : "Checked In"
                })
                .ToListAsync();

            return Ok(checkInOuts);
        }
    }

    // Request DTOs
    public class CreateRoomRequest
    {
        public int RoomNumber { get; set; }
        public string? Availability { get; set; }
        public int LocationId { get; set; }
        public string? Remark { get; set; }
    }

    public class UpdateRoomRequest
    {
        public int RoomNumber { get; set; }
        public string? Availability { get; set; }
        public int LocationId { get; set; }
        public string? Remark { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
