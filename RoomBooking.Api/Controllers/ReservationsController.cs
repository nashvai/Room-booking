using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReservationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ReservationFilter filter)
    {
        var query = _db.Reservations.AsQueryable();

        if (filter.RoomId.HasValue)
        {
            query = query.Where(r =>
                r.RoomId == filter.RoomId.Value);
        }

        if (filter.OrganizerId.HasValue)
        {
            query = query.Where(r =>
                r.UserId == filter.OrganizerId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(r =>
                r.Status == filter.Status.Value);
        }

        // Reservation must end after the requested start boundary.
        if (filter.From.HasValue)
        {
            query = query.Where(r =>
                r.End > filter.From.Value);
        }

        // Reservation must start before the requested end boundary.
        if (filter.To.HasValue)
        {
            query = query.Where(r =>
                r.Start < filter.To.Value);
        }

        var reservations = await query
            .Select(r => new ReservationResponse(
                r.Id,
                r.RoomId,
                r.Room.Name,
                r.UserId,
                r.User.FirstName + " " + r.User.LastName,
                r.Title,
                r.Start,
                r.End,
                r.Attendees,
                r.Status
            ))
            .ToListAsync();

        return Ok(reservations);
    }
}