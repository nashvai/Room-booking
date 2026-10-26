using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;

namespace RoomBooking.Api.Validation;

public class ActiveRoomRule : IBookingRule
{
    public Task<ApiError?> CheckAsync(
        CreateReservationRequest request,
        Room room)
    {
        if (!room.IsActive)
        {
            return Task.FromResult<ApiError?>(
                new ApiError(
                    "ROOM_INACTIVE",
                    "Reservations cannot be created for inactive rooms."));
        }

        return Task.FromResult<ApiError?>(null);
    }
}