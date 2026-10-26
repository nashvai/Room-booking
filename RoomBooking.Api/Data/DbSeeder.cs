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
    }
}