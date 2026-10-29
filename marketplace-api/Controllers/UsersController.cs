using System.Security.Claims;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(int id)
    {
        var result = await _users.GetPublicProfileAsync(id);
        return ToActionResult(result);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _users.UpdateProfileAsync(userId.Value, request);
        return ToActionResult(result);
    }

    [HttpPut("me/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _users.ChangePasswordAsync(userId.Value, request);
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
