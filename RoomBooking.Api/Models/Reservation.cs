namespace RoomBooking.Api.Models;

public class Reservation
{
    public int Id { get; set; }

    // Relationship to Room
    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    // Relationship to User (the organizer)
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public DateTimeOffset Start { get; set; }

    public DateTimeOffset End { get; set; }

    public DateTimeOffset? ActualEndTime { get; set; }

    public int Attendees { get; set; }

    public ReservationStatus Status { get; set; }
        = ReservationStatus.Booked;
}