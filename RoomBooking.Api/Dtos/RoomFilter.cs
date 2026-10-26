using RoomBooking.Api.Models;

namespace RoomBooking.Api.Dtos;

public class RoomFilter
{
    public int? MinCapacity { get; set; }

    public Equipment? Equipment { get; set; }

    public string? Floor { get; set; }
}