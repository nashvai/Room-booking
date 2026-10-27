
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;

namespace RoomBooking.Api.Services;

public class UsageReportService
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _clock;

    public UsageReportService(AppDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<ServiceResult<List<RoomUsageResponse>>> GetRoomUsageAsync(
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        var now = _clock.GetUtcNow();

        var rangeEnd = to?.ToUniversalTime() ?? now;
        var rangeStart = from?.ToUniversalTime()
            ?? rangeEnd.AddDays(-30);

        if (rangeStart >= rangeEnd)
        {
            return ServiceResult<List<RoomUsageResponse>>.Fail(
                FailureKind.Invalid,
                new ApiError(
                    ErrorCodes.InvalidReportRange,
                    "'from' must be earlier than 'to'."));
        }

        var reservations = await _db.Reservations
            .AsNoTracking()
            .Where(r =>
                r.Start < rangeEnd &&
                r.End > rangeStart)
            .ToListAsync();

        var rooms = await _db.Rooms
            .AsNoTracking()
            .ToListAsync();

        var windowHours = (rangeEnd - rangeStart).TotalHours;

        var report = rooms
            .Select(room =>
            {
                var roomReservations = reservations
                    .Where(r => r.RoomId == room.Id)
                    .ToList();

                var nonCancelled = roomReservations
                    .Where(r => r.Status != ReservationStatus.Cancelled)
                    .ToList();

                // Scheduled hours for all non-cancelled reservations.
                var bookedHours = nonCancelled.Sum(r =>
                    ClippedHours(
                        r.Start,
                        r.End,
                        rangeStart,
                        rangeEnd));

                // Finished meetings only: both sides of the usage-rate
                // calculation must use this same set.
                var finished = nonCancelled
                    .Where(r => (r.ActualEndTime ?? r.End) <= now)
                    .ToList();

                var finishedBookedHours = finished.Sum(r =>
                    ClippedHours(
                        r.Start,
                        r.End,
                        rangeStart,
                        rangeEnd));

                var actualHours = finished.Sum(r =>
                    ClippedHours(
                        r.Start,
                        r.ActualEndTime ?? r.End,
                        rangeStart,
                        rangeEnd));

                // Calendar occupancy: an early-ended meeting releases
                // its unused scheduled time.
                var occupiedHours = nonCancelled.Sum(r =>
                {
                    var occupiedEnd = r.ActualEndTime ?? r.End;

                    return ClippedHours(
                        r.Start,
                        occupiedEnd,
                        rangeStart,
                        rangeEnd);
                });

                var cancelledCount = roomReservations.Count(
                    r => r.Status == ReservationStatus.Cancelled);

                var occupancyPercent = windowHours > 0
                    ? occupiedHours / windowHours * 100
                    : 0;

                var usageRatePercent = finishedBookedHours > 0
                    ? actualHours / finishedBookedHours * 100
                    : 0;

                return new RoomUsageResponse(
                    room.Id,
                    room.Name,
                    nonCancelled.Count,
                    cancelledCount,
                    Math.Round(bookedHours, 2),
                    Math.Round(actualHours, 2),
                    Math.Round(occupancyPercent, 2),
                    Math.Round(usageRatePercent, 2));
            })
            .ToList();

        return ServiceResult<List<RoomUsageResponse>>.Ok(report);
    }

    private static double ClippedHours(
        DateTimeOffset start,
        DateTimeOffset end,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd)
    {
        var clippedStart = start > rangeStart ? start : rangeStart;
        var clippedEnd = end < rangeEnd ? end : rangeEnd;

        return Math.Max(0, (clippedEnd - clippedStart).TotalHours);
    }
}
