namespace RoomBooking.Api.Dtos;

public record CreateReservationRequest(
    int RoomId,
    int OrganizerId,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    int Attendees
);