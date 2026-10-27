
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;
using RoomBooking.Api.Services;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _clock;

    public UsersController(AppDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var now = _clock.GetUtcNow();

        var users = await _db.Users
            .AsNoTracking()
            .Select(u => new UserResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Reservations
                    .AsQueryable()
                    .Count(
                        ReservationStatusRules.HasEffectiveStatus(
                            ReservationStatus.Booked,
                            now))
            ))
            .ToListAsync();

        return Ok(users);
    }
}
