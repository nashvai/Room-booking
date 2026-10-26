namespace RoomBooking.Api.Models;

public class Room
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Floor { get; set; } = string.Empty;

    public Equipment Equipment { get; set; } = Equipment.None;

    public bool IsActive { get; set; } = true;

    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();
}