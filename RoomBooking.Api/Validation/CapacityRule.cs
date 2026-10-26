using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Validation;

public class CapacityRule : IBookingRule
{
    public int Order => BookingRuleOrder.Capacity;

    public Task<ApiError?> CheckAsync(
        BookingValidationContext context)
    {
        if (context.Request.Attendees > context.Room.Capacity)
        {
            return Task.FromResult<ApiError?>(
                new ApiError(
                    ErrorCodes.AttendeesExceedCapacity,
                    "The number of attendees exceeds the room's capacity."));
        }

        return Task.FromResult<ApiError?>(null);
    }
}