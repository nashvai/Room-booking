using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Validation;

public class BookingValidationService
{
    private readonly IReadOnlyList<IBookingRule> _rules;

    public BookingValidationService(
        IEnumerable<IBookingRule> rules)
    {
        _rules = rules
            .OrderBy(rule => rule.Order)
            .ToList();
    }

    public async Task<ApiError?> ValidateAsync(
        BookingValidationContext context)
    {
        foreach (var rule in _rules)
        {
            var error = await rule.CheckAsync(context);

            if (error is not null)
            {
                return error;
            }
        }

        return null;
    }
}