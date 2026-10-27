
using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Dtos;
using RoomBooking.Api.Services;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly UsageReportService _usageReportService;

    public ReportsController(UsageReportService usageReportService)
    {
        _usageReportService = usageReportService;
    }

    [HttpGet("room-usage")]
    public async Task<IActionResult> RoomUsage(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        var result = await _usageReportService.GetRoomUsageAsync(from, to);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Failure switch
        {
            FailureKind.Invalid => BadRequest(result.Error),
            FailureKind.NotFound => NotFound(result.Error),
            FailureKind.Conflict => Conflict(result.Error),
            _ => StatusCode(500, result.Error)
        };
    }
}
