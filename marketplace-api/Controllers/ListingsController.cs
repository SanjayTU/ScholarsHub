using System.Security.Claims;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController]
[Route("api/listings")]
[Authorize]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listings;

    public ListingsController(IListingService listings) => _listings = listings;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] ListingSearchParams query)
    {
        var result = await _listings.SearchListingsAsync(query);
        return ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _listings.GetListingDetailsAsync(id);
        return ToActionResult(result);
    }

    [HttpGet("{id:int}/contact")]
    public async Task<IActionResult> GetContact(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _listings.GetSellerContactAsync(id, userId.Value);
        return ToActionResult(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] ListingRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _listings.CreateAsync(userId.Value, request);
        return ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] ListingRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _listings.UpdateAsync(userId.Value, id, request);
        return ToActionResult(result);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeListingStatusRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _listings.ChangeStatusAsync(userId.Value, id, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _listings.DeleteAsync(userId.Value, id);
        return ToActionResult(result);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _listings.GetMyListingsAsync(userId.Value);
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
