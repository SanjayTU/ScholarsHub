using System.Security.Claims;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IChatService _chat;

    public ConversationsController(IChatService chat) => _chat = chat;

    [HttpPost]
    public async Task<IActionResult> Start([FromBody] StartChatRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _chat.StartOrGetConversationAsync(userId.Value, request);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetInbox()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _chat.GetInboxAsync(userId.Value);
        return ToActionResult(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _chat.GetUnreadCountAsync(userId.Value);
        return ToActionResult(result);
    }

    [HttpGet("{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int? after)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _chat.GetMessagesAsync(userId.Value, id, after);
        return ToActionResult(result);
    }

    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { status = 401, message = "Invalid token." });

        var result = await _chat.SendMessageAsync(userId.Value, id, request);
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
