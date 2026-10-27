
using System.Linq.Expressions;
using RoomBooking.Api.Models;

namespace RoomBooking.Api.Services;

public static class ReservationStatusRules
{
    public static ReservationStatus GetEffectiveStatus(
        Reservation reservation,
        DateTimeOffset now)
    {
        if (reservation.Status == ReservationStatus.Booked
            && reservation.End <= now)
        {
            return ReservationStatus.Completed;
        }

        return reservation.Status;
    }

    public static Expression<Func<Reservation, bool>> HasEffectiveStatus(
        ReservationStatus status,
        DateTimeOffset now)
    {
        Expression<Func<Reservation, ReservationStatus>> effectiveStatus =
            r => r.Status == ReservationStatus.Booked && r.End <= now
                ? ReservationStatus.Completed
                : r.Status;

        var parameter = effectiveStatus.Parameters[0];

        var body = Expression.Equal(
            effectiveStatus.Body,
            Expression.Constant(status));

        return Expression.Lambda<Func<Reservation, bool>>(
            body,
            parameter);
    }
}
