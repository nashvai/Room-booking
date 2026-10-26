namespace RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;

public record RoomResponse(
    int Id,
    string Name,
    int Capacity,
    string Floor,
    Equipment Equipment,
    bool IsActive
);