using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Select(u => new UserResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Reservations.Count(r =>
                    r.Status == ReservationStatus.Booked)
            ))
            .ToListAsync();

        return Ok(users);
    }
}