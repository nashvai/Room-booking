namespace RoomBooking.Api.Dtos;

public record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    int ActiveReservations
);