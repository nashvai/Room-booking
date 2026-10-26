using RoomBooking.Api.Models;

namespace RoomBooking.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Rooms.Any())
        {
            db.Rooms.AddRange(
                new Room
                {
                    Name = "Conference Room A",
                    Capacity = 12,
                    Floor = "1st Floor",
                    Equipment = Equipment.Projector | Equipment.Whiteboard
                },
                new Room
                {
                    Name = "Meeting Room B",
                    Capacity = 6,
                    Floor = "2nd Floor",
                    Equipment = Equipment.TvScreen | Equipment.VideoConferencing
                },
                new Room
                {
                    Name = "Quiet Room C",
                    Capacity = 4,
                    Floor = "1st Floor",
                    Equipment = Equipment.None
                },
                new Room
                {
                    Name = "Conference Room D",
                    Capacity = 20,
                    Floor = "3rd Floor",
                    Equipment = Equipment.Projector | Equipment.TvScreen,
                    IsActive = false
                }
            );
        }

        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new User
                {
                    FirstName = "Alex",
                    LastName = "Johnson"
                },
                new User
                {
                    FirstName = "Sam",
                    LastName = "Taylor"
                }
            );
        }

        db.SaveChanges();

        if (!db.Reservations.Any())
{
    var roomA = db.Rooms.First(r => r.Name == "Conference Room A");
    var roomB = db.Rooms.First(r => r.Name == "Meeting Room B");

    var alex = db.Users.First(u => u.FirstName == "Alex");
    var sam = db.Users.First(u => u.FirstName == "Sam");

    var now = DateTimeOffset.UtcNow;

    db.Reservations.AddRange(
        new Reservation
        {
            RoomId = roomA.Id,
            UserId = alex.Id,
            Title = "Project Planning",
            Start = now.AddHours(1),
            End = now.AddHours(2),
            Attendees = 5,
            Status = ReservationStatus.Booked
        },
        new Reservation
        {
            RoomId = roomB.Id,
            UserId = sam.Id,
            Title = "Team Meeting",
            Start = now.AddHours(3),
            End = now.AddHours(4),
            Attendees = 4,
            Status = ReservationStatus.Booked
        }
    );

    db.SaveChanges();
}
    }
}