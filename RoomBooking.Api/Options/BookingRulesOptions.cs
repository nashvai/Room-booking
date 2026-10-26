namespace RoomBooking.Api.Options;

public class BookingRulesOptions
{
    public const string SectionName = "BookingRules";

    public int MaxDurationDays { get; set; }

    public int MaxHorizonDays { get; set; }
}