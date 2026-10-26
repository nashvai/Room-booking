using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Api.Dtos;

public record CreateReservationRequest(
    int RoomId,
    int OrganizerId,

    [Required]
    [StringLength(200)]
    string Title,

    DateTimeOffset Start,
    DateTimeOffset End,

    [Range(1, int.MaxValue)]
    int Attendees
);