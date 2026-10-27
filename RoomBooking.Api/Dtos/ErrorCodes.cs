namespace RoomBooking.Api.Dtos;

public static class ErrorCodes
{
    public const string InvalidRequest = "INVALID_REQUEST";

    public const string RoomNotFound = "ROOM_NOT_FOUND";
    public const string OrganizerNotFound = "ORGANIZER_NOT_FOUND";
    public const string ReservationNotFound = "RESERVATION_NOT_FOUND";

    public const string InvalidTimeRange = "INVALID_TIME_RANGE";
    public const string StartNotInFuture = "START_NOT_IN_FUTURE";
    public const string BookingHorizonExceeded = "BOOKING_HORIZON_EXCEEDED";
    public const string MaxDurationExceeded = "MAX_DURATION_EXCEEDED";
    public const string AttendeesExceedCapacity = "ATTENDEES_EXCEED_CAPACITY";
    public const string RoomInactive = "ROOM_INACTIVE";
    public const string RoomReservationOverlap = "ROOM_RESERVATION_OVERLAP";
    public const string OrganizerReservationOverlap = "ORGANIZER_RESERVATION_OVERLAP";
    public const string InvalidReservationState = "INVALID_RESERVATION_STATE";
}