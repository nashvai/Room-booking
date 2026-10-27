
using RoomBooking.Api.Models;
using RoomBooking.Api.Services;

namespace RoomBooking.Api.Dtos;

public static class ReservationMappings
{
    public static ReservationResponse ToResponse(
        Reservation reservation,
        DateTimeOffset now)
    {
        return new ReservationResponse(
            reservation.Id,
            reservation.RoomId,
            reservation.Room.Name,
            reservation.UserId,
            $"{reservation.User.FirstName} {reservation.User.LastName}",
            reservation.Title,
            reservation.Start,
            reservation.End,
            reservation.Attendees,
            ReservationStatusRules.GetEffectiveStatus(reservation, now)
        );
    }
}
