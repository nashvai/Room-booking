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
public async Task<IActionResult> GetAll(
    [FromQuery] RoomFilter filter)
{
    var query = _db.Rooms.AsQueryable();

    // Minimum capacity
    if (filter.MinCapacity.HasValue)
    {
        query = query.Where(
            r => r.Capacity >= filter.MinCapacity.Value);
    }

    // Must have ALL requested equipment items
    if (filter.Equipment.HasValue)
    {
        var requestedEquipment = filter.Equipment.Value;

        query = query.Where(r =>
            (r.Equipment & requestedEquipment)
            == requestedEquipment);
    }

    // Exact floor match
    if (!string.IsNullOrWhiteSpace(filter.Floor))
    {
        query = query.Where(
            r => r.Floor == filter.Floor);
    }

    var rooms = await query
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