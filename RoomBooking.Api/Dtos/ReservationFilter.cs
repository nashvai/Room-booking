using RoomBooking.Api.Models;

namespace RoomBooking.Api.Dtos;

public class ReservationFilter
{
    public int? RoomId { get; set; }

    public int? OrganizerId { get; set; }

    public ReservationStatus? Status { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }
}