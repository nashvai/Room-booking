using Microsoft.Extensions.Options;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Options;

namespace RoomBooking.Api.Validation;

public class MaxDurationRule : IBookingRule
{
    private readonly BookingRulesOptions _options;

    public int Order => BookingRuleOrder.MaxDuration;

    public MaxDurationRule(
        IOptions<BookingRulesOptions> options)
    {
        _options = options.Value;
    }

    public Task<ApiError?> CheckAsync(
        BookingValidationContext context)
    {
        var duration =
            context.Request.End - context.Request.Start;

        var maxDuration =
            TimeSpan.FromDays(_options.MaxDurationDays);

        if (duration > maxDuration)
        {
            return Task.FromResult<ApiError?>(
                new ApiError(
                    ErrorCodes.MaxDurationExceeded,
                    $"A reservation cannot exceed {_options.MaxDurationDays} days."));
        }

        return Task.FromResult<ApiError?>(null);
    }
}