namespace RoomBooking.Api.Models;

public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();
}