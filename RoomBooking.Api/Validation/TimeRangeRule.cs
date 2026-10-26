using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Validation;

public class TimeRangeRule : IBookingRule
{
    private readonly TimeProvider _clock;

    public int Order => BookingRuleOrder.TimeRange;

    public TimeRangeRule(TimeProvider clock)
    {
        _clock = clock;
    }

    public Task<ApiError?> CheckAsync(
        BookingValidationContext context)
    {
        var request = context.Request;

        if (request.End <= request.Start)
        {
            return Task.FromResult<ApiError?>(
                new ApiError(
                    ErrorCodes.InvalidTimeRange,
                    "The reservation end time must be after its start time."));
        }

        if (request.Start <= _clock.GetUtcNow())
        {
            return Task.FromResult<ApiError?>(
                new ApiError(
                    ErrorCodes.StartNotInFuture,
                    "The reservation must start in the future."));
        }

        return Task.FromResult<ApiError?>(null);
    }
}