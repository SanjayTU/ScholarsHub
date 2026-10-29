using System.Security.Claims;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController]
[Route("api/listings")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    [HttpPost("{id:int}/report")]
    public async Task<IActionResult> Create(int id, [FromBody] ReportRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _reports.CreateReportAsync(userId.Value, id, request);
        return ToActionResult(result);
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : null;
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (!result.Success)
            return StatusCode(result.Status, new { status = result.Status, message = result.Message, errors = result.Errors });
        return StatusCode(result.Status, result.Data);
    }
}
