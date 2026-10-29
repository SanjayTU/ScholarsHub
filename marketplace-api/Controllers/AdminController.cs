using System.Security.Claims;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Models.Enums;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin) => _admin = admin;

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() =>
        ToActionResult(await _admin.GetDashboardStatsAsync());

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 12) =>
        ToActionResult(await _admin.GetUsersAsync(q, page, pageSize));

    [HttpPatch("users/{id:int}/status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var adminId = GetUserId();
        if (adminId is null) return Unauthorized(new { status = 401, message = "Invalid token." });
        return ToActionResult(await _admin.ToggleUserStatusAsync(adminId.Value, id));
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var adminId = GetUserId();
        if (adminId is null) return Unauthorized(new { status = 401, message = "Invalid token." });
        return ToActionResult(await _admin.DeleteUserAsync(adminId.Value, id));
    }

    [HttpGet("listings")]
    public async Task<IActionResult> GetListings([FromQuery] string? q, [FromQuery] ListingStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 12) =>
        ToActionResult(await _admin.GetListingsAsync(q, status, page, pageSize));

    [HttpPatch("listings/{id:int}/status")]
    public async Task<IActionResult> RemoveListing(int id) =>
        ToActionResult(await _admin.RemoveListingAsync(id));

    [HttpDelete("listings/{id:int}")]
    public async Task<IActionResult> DeleteListing(int id) =>
        ToActionResult(await _admin.RemoveListingAsync(id));

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories() =>
        ToActionResult(await _admin.GetCategoriesAsync());

    [HttpPost("categories")]
    public async Task<IActionResult> AddCategory([FromBody] AdminCategoryRequest request) =>
        ToActionResult(await _admin.AddCategoryAsync(request));

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] AdminCategoryRequest request) =>
        ToActionResult(await _admin.UpdateCategoryAsync(id, request));

    [HttpPatch("categories/{id:int}/status")]
    public async Task<IActionResult> ToggleCategory(int id) =>
        ToActionResult(await _admin.ToggleCategoryStatusAsync(id));

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeactivateCategory(int id) =>
        ToActionResult(await _admin.ToggleCategoryStatusAsync(id));

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports([FromQuery] ReportStatus? status) =>
        ToActionResult(await _admin.GetReportsAsync(status));

    [HttpPatch("reports/{id:int}")]
    public async Task<IActionResult> HandleReport(int id, [FromBody] AdminReportActionRequest request)
    {
        var adminId = GetUserId();
        if (adminId is null) return Unauthorized(new { status = 401, message = "Invalid token." });
        return ToActionResult(await _admin.HandleReportAsync(adminId.Value, id, request));
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (!result.Success)
            return StatusCode(result.Status, new { status = result.Status, message = result.Message, errors = result.Errors });
        return StatusCode(result.Status, result.Data);
    }
}
