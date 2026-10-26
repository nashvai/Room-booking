using Microsoft.Extensions.Options;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Options;

namespace RoomBooking.Api.Validation;

public class BookingHorizonRule : IBookingRule
{
    private readonly BookingRulesOptions _options;
    private readonly TimeProvider _clock;

    public int Order => BookingRuleOrder.BookingHorizon;

    public BookingHorizonRule(
        IOptions<BookingRulesOptions> options,
        TimeProvider clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public Task<ApiError?> CheckAsync(
        BookingValidationContext context)
    {
        var latestAllowedStart = _clock.GetUtcNow()
            .AddDays(_options.MaxHorizonDays);

        if (context.Request.Start > latestAllowedStart)
        {
            return Task.FromResult<ApiError?>(
                new ApiError(
                    ErrorCodes.BookingHorizonExceeded,
                    $"Reservations cannot be made more than {_options.MaxHorizonDays} days in advance."));
        }

        return Task.FromResult<ApiError?>(null);
    }
}