using Microsoft.Extensions.Options;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;
using RoomBooking.Api.Options;

namespace RoomBooking.Api.Validation;

public class MaxDurationRule : IBookingRule
{
    private readonly BookingRulesOptions _options;

    public MaxDurationRule(
        IOptions<BookingRulesOptions> options)
    {
        _options = options.Value;
    }

    public Task<ApiError?> CheckAsync(
        CreateReservationRequest request,
        Room room)
    {
        var duration = request.End - request.Start;
        var maxDuration = TimeSpan.FromDays(
            _options.MaxDurationDays);

        if (duration > maxDuration)
        {
            return Task.FromResult<ApiError?>(
                new ApiError(
                    "MAX_DURATION_EXCEEDED",
                    $"A reservation cannot exceed {_options.MaxDurationDays} days."));
        }

        return Task.FromResult<ApiError?>(null);
    }
}