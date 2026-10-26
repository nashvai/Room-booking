using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _db;

    public RoomsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [HttpGet]
public async Task<IActionResult> GetAll()
{
    var rooms = await _db.Rooms
        .Select(r => new RoomResponse(
            r.Id,
            r.Name,
            r.Capacity,
            r.Floor,
            r.Equipment,
            r.IsActive
        ))
        .ToListAsync();

    return Ok(rooms);
}
}