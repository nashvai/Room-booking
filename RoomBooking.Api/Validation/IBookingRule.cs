using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Validation;

public interface IBookingRule
{
    int Order { get; }

    Task<ApiError?> CheckAsync(
        BookingValidationContext context);
}