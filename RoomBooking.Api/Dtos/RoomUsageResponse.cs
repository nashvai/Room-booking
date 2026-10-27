
namespace RoomBooking.Api.Dtos;

public record RoomUsageResponse(
    int RoomId,
    string RoomName,
    int BookingCount,
    int CancelledCount,
    double BookedHours,
    double ActualHours,
    double OccupancyPercent,
    double UsageRatePercent);
