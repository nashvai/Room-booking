
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

        // Save rooms and users so they can be looked up by name.
        db.SaveChanges();

        // Seed reservations only when none exist.
      
if (!db.Reservations.Any())
{
    var roomA = db.Rooms
        .First(r => r.Name == "Conference Room A");

    var roomB = db.Rooms
        .First(r => r.Name == "Meeting Room B");

    var quietRoom = db.Rooms
        .First(r => r.Name == "Quiet Room C");

    var alex = db.Users
        .First(u => u.FirstName == "Alex"
            && u.LastName == "Johnson");

    var sam = db.Users
        .First(u => u.FirstName == "Sam"
            && u.LastName == "Taylor");

    var now = DateTimeOffset.UtcNow;

    db.Reservations.AddRange(
        // Room A: several meetings ended early.
        // High booking count, low usage rate.
        new Reservation
        {
            RoomId = roomA.Id,
            UserId = alex.Id,
            Title = "Planning Meeting 1",
            Start = now.AddDays(-10).AddHours(-4),
            End = now.AddDays(-10).AddHours(-2),
            ActualEndTime = now.AddDays(-10).AddHours(-3.5),
            Attendees = 5,
            Status = ReservationStatus.Completed
        },
        new Reservation
        {
            RoomId = roomA.Id,
            UserId = sam.Id,
            Title = "Planning Meeting 2",
            Start = now.AddDays(-8).AddHours(-5),
            End = now.AddDays(-8).AddHours(-3),
            ActualEndTime = now.AddDays(-8).AddHours(-4.5),
            Attendees = 4,
            Status = ReservationStatus.Completed
        },
        new Reservation
        {
            RoomId = roomA.Id,
            UserId = alex.Id,
            Title = "Planning Meeting 3",
            Start = now.AddDays(-6).AddHours(-6),
            End = now.AddDays(-6).AddHours(-4),
            ActualEndTime = now.AddDays(-6).AddHours(-5.5),
            Attendees = 6,
            Status = ReservationStatus.Completed
        },

        // Room B: meetings ran to their scheduled ends.
        // Usage rate should be 100%.
        new Reservation
        {
            RoomId = roomB.Id,
            UserId = sam.Id,
            Title = "Team Meeting 1",
            Start = now.AddDays(-9).AddHours(-4),
            End = now.AddDays(-9).AddHours(-2),
            Attendees = 4,
            Status = ReservationStatus.Completed
        },
        new Reservation
        {
            RoomId = roomB.Id,
            UserId = alex.Id,
            Title = "Team Meeting 2",
            Start = now.AddDays(-7).AddHours(-5),
            End = now.AddDays(-7).AddHours(-3),
            Attendees = 5,
            Status = ReservationStatus.Completed
        },
        new Reservation
        {
            RoomId = roomB.Id,
            UserId = sam.Id,
            Title = "Team Meeting 3",
            Start = now.AddDays(-5).AddHours(-3),
            End = now.AddDays(-5).AddHours(-1),
            Attendees = 3,
            Status = ReservationStatus.Completed
        },

        // Quiet Room C: one fully used booking.
        new Reservation
        {
            RoomId = quietRoom.Id,
            UserId = alex.Id,
            Title = "Quiet Work Session",
            Start = now.AddDays(-4).AddHours(-4),
            End = now.AddDays(-4).AddHours(-3),
            Attendees = 2,
            Status = ReservationStatus.Completed
        },

        // Cancelled reservation: counted separately, no usage hours.
        new Reservation
        {
            RoomId = roomA.Id,
            UserId = sam.Id,
            Title = "Cancelled Planning Session",
            Start = now.AddDays(-3).AddHours(-4),
            End = now.AddDays(-3).AddHours(-2),
            Attendees = 3,
            Status = ReservationStatus.Cancelled
        }
    );

    db.SaveChanges();
}

        }
}

