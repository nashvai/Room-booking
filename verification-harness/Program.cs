using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Controllers;
using RoomBooking.Api.Data;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Models;
using RoomBooking.Api.Services;
using RoomBooking.Api.Validation;

static void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException($"FAIL: {name}");
    Console.WriteLine($"PASS: {name}");
}

static ObjectResult AsObjectResult(IActionResult result) =>
    result as ObjectResult ?? throw new InvalidOperationException($"Expected ObjectResult, got {result.GetType().Name}");

var now = new DateTimeOffset(2026, 10, 27, 16, 0, 0, TimeSpan.Zero);
var clock = new FixedTimeProvider(now);
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;

await using var db = new AppDbContext(options);
var room = new Room { Name = "Test Room", Capacity = 10, Floor = "1st Floor" };
var user = new User { FirstName = "Test", LastName = "User" };
db.AddRange(room, user);
await db.SaveChangesAsync();

Reservation Make(string title, DateTimeOffset start, DateTimeOffset end, ReservationStatus status = ReservationStatus.Booked, DateTimeOffset? actualEnd = null) => new()
{
    RoomId = room.Id, Room = room, UserId = user.Id, User = user, Title = title,
    Start = start, End = end, ActualEndTime = actualEnd, Attendees = 1, Status = status
};

var overdue = Make("Overdue", now.AddHours(-2), now.AddHours(-1));
var boundary = Make("Exact boundary", now.AddHours(-1), now);
var inProgress = Make("In progress", now.AddMinutes(-10), now.AddHours(1));
var explicitCompleted = Make("Completed", now.AddHours(-4), now.AddHours(-3), ReservationStatus.Completed);
var cancelled = Make("Cancelled", now.AddDays(1), now.AddDays(1).AddHours(1), ReservationStatus.Cancelled);
db.Reservations.AddRange(overdue, boundary, inProgress, explicitCompleted, cancelled);
await db.SaveChangesAsync();

var validator = new BookingValidationService(Array.Empty<IBookingRule>());
var reservations = new ReservationsController(db, validator, clock);
var all = (List<ReservationResponse>)AsObjectResult(await reservations.GetAll(new ReservationFilter())).Value!;
Check(all.Single(x => x.Title == "Overdue").Status == ReservationStatus.Completed, "overdue stored Booked reservation reports as Completed");
Check(all.Single(x => x.Title == "Exact boundary").Status == ReservationStatus.Completed, "End exactly equal to now reports as Completed");
Check(all.Single(x => x.Title == "In progress").Status == ReservationStatus.Booked, "in-progress reservation reports as Booked");
var booked = (List<ReservationResponse>)AsObjectResult(await reservations.GetAll(new ReservationFilter { Status = ReservationStatus.Booked })).Value!;
var completed = (List<ReservationResponse>)AsObjectResult(await reservations.GetAll(new ReservationFilter { Status = ReservationStatus.Completed })).Value!;
Check(booked.All(x => x.Title != "Overdue" && x.Title != "Exact boundary"), "effective Booked filter excludes overdue and exact-boundary reservations");
Check(completed.Any(x => x.Title == "Overdue") && completed.Any(x => x.Title == "Exact boundary"), "effective Completed filter includes overdue and exact-boundary reservations");

var users = new UsersController(db, clock);
var userRows = (List<UserResponse>)AsObjectResult(await users.GetAll()).Value!;
Check(userRows.Single().ActiveReservations == 1, "active user count includes in-progress Booked, excludes overdue and End == now");

async Task CheckConflict(string label, Task<ActionResult<ReservationResponse>> action)
{
    var actionResult = await action;
    var result = AsObjectResult(actionResult.Result!);
    Check(result.StatusCode == 409, $"{label} returns HTTP 409");
}

await CheckConflict("ending a completed reservation", reservations.EndEarly(explicitCompleted.Id));
await CheckConflict("ending a cancelled reservation", reservations.EndEarly(cancelled.Id));
await CheckConflict("ending an overdue reservation", reservations.EndEarly(overdue.Id));
await CheckConflict("cancelling a completed reservation", reservations.Cancel(explicitCompleted.Id));
await CheckConflict("cancelling a cancelled reservation", reservations.Cancel(cancelled.Id));
await CheckConflict("cancelling an in-progress reservation", reservations.Cancel(inProgress.Id));
await CheckConflict("cancelling an overdue Booked reservation", reservations.Cancel(overdue.Id));

await using var reportDb = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
var reportRoom = new Room { Name = "Reuse Test Room", Capacity = 10, Floor = "1st Floor" };
var reportUser = new User { FirstName = "Report", LastName = "User" };
reportDb.AddRange(reportRoom, reportUser);
await reportDb.SaveChangesAsync();
var windowStart = new DateTimeOffset(2026, 10, 27, 10, 0, 0, TimeSpan.Zero);
reportDb.Reservations.AddRange(
    new Reservation { RoomId = reportRoom.Id, UserId = reportUser.Id, Title = "Ended early", Start = windowStart, End = windowStart.AddHours(2), ActualEndTime = windowStart.AddMinutes(30), Attendees = 1, Status = ReservationStatus.Completed },
    new Reservation { RoomId = reportRoom.Id, UserId = reportUser.Id, Title = "Reused slot", Start = windowStart.AddMinutes(45), End = windowStart.AddHours(2), Attendees = 1, Status = ReservationStatus.Completed });
await reportDb.SaveChangesAsync();
var report = await new UsageReportService(reportDb, new FixedTimeProvider(windowStart.AddHours(3)))
    .GetRoomUsageAsync(windowStart, windowStart.AddHours(2));
var usage = report.Value!.Single();
Check(usage.BookedHours == 3.25 && usage.ActualHours == 1.75, "reused early-released slot books 3.25 scheduled hours but occupies 1.75 actual hours");
Check(usage.OccupancyPercent == 87.5 && usage.OccupancyPercent <= 100, "reused early-released slot occupancy is 87.5%, never over 100%");
Check(usage.UsageRatePercent == 53.85, "usage rate uses the finished meetings' common scheduled-hours denominator");

sealed class FixedTimeProvider(DateTimeOffset fixedNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => fixedNow;
}
