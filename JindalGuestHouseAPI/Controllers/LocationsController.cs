using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JindalGuestHouseAPI.Data;
using JindalGuestHouseAPI.Models;
using ApiLocation = JindalGuestHouseAPI.Models.Location;

namespace JindalGuestHouseAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly GuestHouseDbContext _context;
        private readonly ILogger<LocationsController> _logger;

        public LocationsController(GuestHouseDbContext context, ILogger<LocationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLocations()
        {
            var locations = await _context.Locations
                .Where(l => l.IsActive)
                .Include(l => l.Rooms.Where(r => r.IsActive))
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.LocationCode,
                    l.Address,
                    l.Remark,
                    l.CreatedAt,
                    l.IsActive,
                    RoomCount = l.Rooms.Count,
                    AvailableRooms = l.Rooms.Count(r => r.Availability == "Available")
                })
                .ToListAsync();

            return Ok(locations);
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetLocation(int id)
        {
            var location = await _context.Locations
                .Include(l => l.Rooms.Where(r => r.IsActive))
                .Where(l => l.Id == id && l.IsActive)
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.LocationCode,
                    l.Address,
                    l.Remark,
                    l.CreatedAt,
                    l.IsActive,
                    Rooms = l.Rooms.Select(r => new
                    {
                        r.Id,
                        r.RoomNumber,
                        r.Availability,
                        r.Remark,
                        r.IsActive
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (location == null)
            {
                return NotFound(new { message = "Location not found" });
            }

            return Ok(location);
        }

        // GET: api/Locations/search?query=main
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchLocations([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetLocations();
            }

            var locations = await _context.Locations
                .Where(l => l.IsActive && 
                           (l.Name.Contains(query) || 
                            l.LocationCode.Contains(query) || 
                            l.Address.Contains(query)))
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.LocationCode,
                    l.Address,
                    l.Remark,
                    l.CreatedAt,
                    l.IsActive
                })
                .ToListAsync();

            return Ok(locations);
        }

        // POST: api/Locations
        [HttpPost]
        public async Task<ActionResult<ApiLocation>> CreateLocation(CreateLocationRequest request)
        {
            try
            {
                // Check if location code already exists
                if (await _context.Locations.AnyAsync(l => l.LocationCode == request.LocationCode))
                {
                    return BadRequest(new { message = "Location code already exists" });
                }

                var location = new ApiLocation
                {
                    Name = request.Name,
                    LocationCode = request.LocationCode,
                    Address = request.Address,
                    Remark = request.Remark,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating location");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT: api/Locations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, UpdateLocationRequest request)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                {
                    return NotFound(new { message = "Location not found" });
                }

                // Check if location code already exists for another location
                if (request.LocationCode != location.LocationCode && 
                    await _context.Locations.AnyAsync(l => l.LocationCode == request.LocationCode && l.Id != id))
                {
                    return BadRequest(new { message = "Location code already exists" });
                }

                location.Name = request.Name;
                location.LocationCode = request.LocationCode;
                location.Address = request.Address;
                location.Remark = request.Remark;
                location.IsActive = request.IsActive;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location {LocationId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var location = await _context.Locations
                    .Include(l => l.Rooms)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (location == null)
                {
                    return NotFound(new { message = "Location not found" });
                }

                // Check if location has active rooms
                if (location.Rooms.Any(r => r.IsActive))
                {
                    return BadRequest(new { message = "Cannot delete location with active rooms" });
                }

                // Soft delete
                location.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Location deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting location {LocationId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET: api/Locations/5/rooms
        [HttpGet("{id}/rooms")]
        public async Task<ActionResult<IEnumerable<object>>> GetLocationRooms(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound(new { message = "Location not found" });
            }

            var rooms = await _context.Rooms
                .Where(r => r.LocationId == id && r.IsActive)
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
    }

    // Request DTOs
    public class CreateLocationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    public class UpdateLocationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
