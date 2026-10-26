using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;

namespace RoomBooking.Api.Validation;

public class BookingValidationContext
{
    public required CreateReservationRequest Request { get; init; }

    public required Room Room { get; init; }

    public int? ExcludeReservationId { get; init; }
}