using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Validation;

public class ActiveRoomRule : IBookingRule
{
    public int Order => BookingRuleOrder.ActiveRoom;

    public Task<ApiError?> CheckAsync(
        BookingValidationContext context)
    {
        if (!context.Room.IsActive)
        {
            return Task.FromResult<ApiError?>(
                new ApiError(
                    ErrorCodes.RoomInactive,
                    "The room is inactive and cannot be booked."));
        }

        return Task.FromResult<ApiError?>(null);
    }
}