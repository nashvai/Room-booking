using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;

namespace RoomBooking.Api.Validation;

public interface IBookingRule
{
    Task<ApiError?> CheckAsync(
        CreateReservationRequest request,
        Room room);
}