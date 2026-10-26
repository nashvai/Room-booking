using RoomBooking.Api.Models;

namespace RoomBooking.Api.Dtos;

public record ReservationResponse(
    int Id,
    int RoomId,
    string RoomName,
    int OrganizerId,
    string OrganizerName,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    int Attendees,
    ReservationStatus Status
);