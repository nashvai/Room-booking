using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;

namespace RoomBooking.Api.Validation;

public class OrganizerOverlapRule : IBookingRule
{
    private readonly AppDbContext _db;

    public int Order => BookingRuleOrder.OrganizerOverlap;

    public OrganizerOverlapRule(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ApiError?> CheckAsync(
        BookingValidationContext context)
    {
        var request = context.Request;

        var query = _db.Reservations.Where(r =>
            r.UserId == request.OrganizerId &&
            r.Status != ReservationStatus.Cancelled &&
            (r.ActualEndTime ?? r.End) > request.Start &&
            r.Start < request.End);

        // When editing, exclude the reservation being edited.
        if (context.ExcludeReservationId.HasValue)
        {
            var excludeId = context.ExcludeReservationId.Value;

            query = query.Where(r => r.Id != excludeId);
        }

        var hasConflict = await query.AnyAsync();

        if (hasConflict)
        {
         return new ApiError(
    ErrorCodes.OrganizerReservationOverlap,
    "The organizer already has a reservation during the requested time.");
        }

        return null;
    }
}