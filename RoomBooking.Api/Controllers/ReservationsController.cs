
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;
using RoomBooking.Api.Services;
using RoomBooking.Api.Validation;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly BookingValidationService _validation;
    private readonly TimeProvider _clock;

    public ReservationsController(
        AppDbContext db,
        BookingValidationService validation,
        TimeProvider clock)
    {
        _db = db;
        _validation = validation;
        _clock = clock;
    }

    // GET /api/reservations
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ReservationFilter filter)
    {
        var now = _clock.GetUtcNow();

        var query = _db.Reservations
            .AsNoTracking()
            .Include(r => r.Room)
            .Include(r => r.User)
            .AsQueryable();

        if (filter.RoomId.HasValue)
        {
            query = query.Where(
                r => r.RoomId == filter.RoomId.Value);
        }

        if (filter.OrganizerId.HasValue)
        {
            query = query.Where(
                r => r.UserId == filter.OrganizerId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(
                ReservationStatusRules.HasEffectiveStatus(
                    filter.Status.Value,
                    now));
        }

        if (filter.From.HasValue)
        {
            query = query.Where(
                r => (r.ActualEndTime ?? r.End) > filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(
                r => r.Start < filter.To.Value);
        }

        var reservations = await query
            .OrderBy(r => r.Start)
            .ToListAsync();

        var response = reservations
            .Select(r => ReservationMappings.ToResponse(r, now))
            .ToList();

        return Ok(response);
    }

    // GET /api/reservations/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reservation = await _db.Reservations
            .AsNoTracking()
            .Include(r => r.Room)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null)
        {
            return NotFound(new ApiError(
                ErrorCodes.ReservationNotFound,
                $"Reservation with ID {id} was not found."));
        }

        return Ok(ReservationMappings.ToResponse(
            reservation,
            _clock.GetUtcNow()));
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

        // Normalize reservation times to UTC and trim the title.
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
                error.Code == ErrorCodes.RoomReservationOverlap
                || error.Code == ErrorCodes.OrganizerReservationOverlap;

            if (isConflict)
            {
                return Conflict(error);
            }

            return BadRequest(error);
        }

        // Create the reservation entity.
        var reservation = new Reservation
        {
            RoomId = room.Id,
            UserId = organizer.Id,
            Room = room,
            User = organizer,
            Title = normalizedRequest.Title,
            Start = normalizedRequest.Start,
            End = normalizedRequest.End,
            Attendees = normalizedRequest.Attendees,
            Status = ReservationStatus.Booked
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = reservation.Id },
            ReservationMappings.ToResponse(
                reservation,
                _clock.GetUtcNow()));
    }

    // POST /api/reservations/1/cancel
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ReservationResponse>> Cancel(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Room)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null)
        {
            return NotFound(new ApiError(
                ErrorCodes.ReservationNotFound,
                "The reservation does not exist."));
        }

        var now = _clock.GetUtcNow();

        // Only future booked reservations can be cancelled.
        if (reservation.Status != ReservationStatus.Booked
            || reservation.Start <= now)
        {
            return Conflict(new ApiError(
                ErrorCodes.InvalidReservationState,
                "Only future booked reservations can be cancelled."));
        }

        reservation.Status = ReservationStatus.Cancelled;

        await _db.SaveChangesAsync();

        return Ok(ReservationMappings.ToResponse(reservation, now));
    }

    // POST /api/reservations/1/end
    [HttpPost("{id:int}/end")]
    public async Task<ActionResult<ReservationResponse>> EndEarly(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Room)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null)
        {
            return NotFound(new ApiError(
                ErrorCodes.ReservationNotFound,
                "The reservation does not exist."));
        }

        var now = _clock.GetUtcNow();

        // Only a currently in-progress booking can be ended early.
        if (reservation.Status != ReservationStatus.Booked
            || reservation.Start > now
            || reservation.End <= now)
        {
            return Conflict(new ApiError(
                ErrorCodes.InvalidReservationState,
                "Only a currently in-progress reservation can be ended early."));
        }

        // Preserve the scheduled End time.
        reservation.ActualEndTime = now;
        reservation.Status = ReservationStatus.Completed;

        await _db.SaveChangesAsync();

        return Ok(ReservationMappings.ToResponse(reservation, now));
    }
}
