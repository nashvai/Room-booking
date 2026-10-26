using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;
using RoomBooking.Api.Validation;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly BookingValidationService _validation;

    public ReservationsController(
        AppDbContext db,
        BookingValidationService validation)
    {
        _db = db;
        _validation = validation;
    }

    // GET /api/reservations
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

        // Include reservations that overlap the requested range.
        if (filter.From.HasValue)
        {
            query = query.Where(r =>
                (r.ActualEndTime ?? r.End) > filter.From.Value);
        }

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

    // GET /api/reservations/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reservation = await _db.Reservations
            .Where(r => r.Id == id)
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
            .FirstOrDefaultAsync();

        if (reservation is null)
        {
            return NotFound(new ApiError(
                ErrorCodes.ReservationNotFound,
                "The requested reservation does not exist."));
        }

        return Ok(reservation);
    }

    // POST /api/reservations
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReservationRequest request)
    {
        // Reject whitespace-only titles.
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new ApiError(
                ErrorCodes.InvalidRequest,
                "Title must not be empty or whitespace."));
        }

        // Look up the room.
        var room = await _db.Rooms.FindAsync(request.RoomId);

        if (room is null)
        {
            return NotFound(new ApiError(
                ErrorCodes.RoomNotFound,
                "The requested room does not exist."));
        }

        // Look up the organizer.
        var organizer = await _db.Users.FindAsync(
            request.OrganizerId);

        if (organizer is null)
        {
            return NotFound(new ApiError(
                ErrorCodes.OrganizerNotFound,
                "The requested organizer does not exist."));
        }

        // Normalize reservation times to UTC.
        var normalizedRequest = request with
        {
            Start = request.Start.ToUniversalTime(),
            End = request.End.ToUniversalTime(),
            Title = request.Title.Trim()
        };

        // Validate the booking.
        var context = new BookingValidationContext
        {
            Request = normalizedRequest,
            Room = room
        };

        var error = await _validation.ValidateAsync(context);

        if (error is not null)
        {
            var isConflict =
                error.Code == ErrorCodes.RoomReservationOverlap ||
                error.Code == ErrorCodes.OrganizerReservationOverlap;

            if (isConflict)
            {
                return Conflict(error);
            }

            return BadRequest(error);
        }

        // Map the validated request to an entity.
        var reservation = new Reservation
        {
            RoomId = room.Id,
            UserId = organizer.Id,
            Title = normalizedRequest.Title,
            Start = normalizedRequest.Start,
            End = normalizedRequest.End,
            Attendees = normalizedRequest.Attendees,
            Status = ReservationStatus.Booked
        };

        // Save.
        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        // Map the saved entity to the response DTO.
        var response = new ReservationResponse(
            reservation.Id,
            reservation.RoomId,
            room.Name,
            organizer.Id,
            $"{organizer.FirstName} {organizer.LastName}",
            reservation.Title,
            reservation.Start,
            reservation.End,
            reservation.Attendees,
            reservation.Status
        );

        // Return 201 Created with a URL to GET the new reservation.
        return CreatedAtAction(
            nameof(GetById),
            new { id = reservation.Id },
            response);
    }
}