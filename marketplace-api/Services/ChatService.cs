using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _db;

    public ChatService(AppDbContext db) => _db = db;

    public async Task<ServiceResult<ConversationDto>> StartOrGetConversationAsync(int userId, StartChatRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user is null)
            return ServiceResult<ConversationDto>.Fail(404, "User not found.");
        if (user.Status != UserStatus.Active)
            return ServiceResult<ConversationDto>.Fail(403, "Account is not active.");

        var listing = await _db.Listings
            .Include(l => l.Images)
            .Include(l => l.Seller)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId);

        if (listing is null)
            return ServiceResult<ConversationDto>.Fail(404, "Listing not found.");

        if (listing.SellerId == userId)
            return ServiceResult<ConversationDto>.Fail(403, "You cannot start a chat on your own listing.");

        var existing = await _db.Conversations
            .Include(c => c.Listing).ThenInclude(l => l.Images)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.ListingId == listing.Id && c.BuyerId == userId);

        if (existing is not null)
            return ServiceResult<ConversationDto>.Ok(await MapConversationAsync(existing, userId));

        var now = DateTime.UtcNow;
        var conversation = new Conversation
        {
            ListingId = listing.Id,
            BuyerId = userId,
            SellerId = listing.SellerId,
            CreatedAt = now,
            LastMessageAt = now
        };

        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync();

        conversation.Listing = listing;
        conversation.Buyer = user;
        conversation.Seller = listing.Seller;

        return ServiceResult<ConversationDto>.Ok(await MapConversationAsync(conversation, userId), 201);
    }

    public async Task<ServiceResult<IReadOnlyList<ConversationDto>>> GetInboxAsync(int userId)
    {
        var conversations = await _db.Conversations
            .AsNoTracking()
            .Include(c => c.Listing).ThenInclude(l => l.Images)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Messages)
            .Where(c => c.BuyerId == userId || c.SellerId == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        var inbox = new List<ConversationDto>(conversations.Count);
        foreach (var conversation in conversations)
            inbox.Add(await MapConversationAsync(conversation, userId));

        return ServiceResult<IReadOnlyList<ConversationDto>>.Ok(inbox);
    }

    public async Task<ServiceResult<IReadOnlyList<MessageDto>>> GetMessagesAsync(int userId, int conversationId, int? after = null)
    {
        var conversation = await _db.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation is null)
            return ServiceResult<IReadOnlyList<MessageDto>>.Fail(404, "Conversation not found.");

        if (conversation.BuyerId != userId && conversation.SellerId != userId)
            return ServiceResult<IReadOnlyList<MessageDto>>.Fail(403, "You are not a participant in this conversation.");

        var unread = await _db.Messages
            .Where(m => m.ConversationId == conversationId && !m.IsRead && m.SenderId != userId)
            .ToListAsync();

        if (unread.Count > 0)
        {
            foreach (var message in unread)
                message.IsRead = true;
            await _db.SaveChangesAsync();
        }

        var query = _db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId);

        if (after.HasValue)
            query = query.Where(m => m.Id > after.Value);

        var messages = await query
            .OrderBy(m => m.SentAt)
            .ThenBy(m => m.Id)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                Body = m.Body,
                SentAt = m.SentAt,
                IsRead = m.IsRead
            })
            .ToListAsync();

        return ServiceResult<IReadOnlyList<MessageDto>>.Ok(messages);
    }

    public async Task<ServiceResult<MessageDto>> SendMessageAsync(int userId, int conversationId, SendMessageRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user is null)
            return ServiceResult<MessageDto>.Fail(404, "User not found.");
        if (user.Status != UserStatus.Active)
            return ServiceResult<MessageDto>.Fail(403, "Account is not active.");

        var conversation = await _db.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation is null)
            return ServiceResult<MessageDto>.Fail(404, "Conversation not found.");

        if (conversation.BuyerId != userId && conversation.SellerId != userId)
            return ServiceResult<MessageDto>.Fail(403, "You are not a participant in this conversation.");

        var now = DateTime.UtcNow;
        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = userId,
            Body = request.Body.Trim(),
            IsRead = false,
            SentAt = now
        };

        conversation.LastMessageAt = now;
        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        return ServiceResult<MessageDto>.Ok(new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            Body = message.Body,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        }, 201);
    }

    public async Task<ServiceResult<object>> GetUnreadCountAsync(int userId)
    {
        var count = await _db.Messages
            .Where(m => !m.IsRead
                        && m.SenderId != userId
                        && (m.Conversation.BuyerId == userId || m.Conversation.SellerId == userId))
            .CountAsync();

        return ServiceResult<object>.Ok(new { unreadCount = count });
    }

    private static Task<ConversationDto> MapConversationAsync(Conversation conversation, int userId)
    {
        var isBuyer = conversation.BuyerId == userId;
        var other = isBuyer ? conversation.Seller : conversation.Buyer;
        var lastMessage = conversation.Messages
            .OrderByDescending(m => m.SentAt)
            .ThenByDescending(m => m.Id)
            .FirstOrDefault();

        var thumbnail = conversation.Listing.Images
            .OrderBy(i => i.SortOrder)
            .Select(i => "/" + i.ImagePath.Replace('\\', '/'))
            .FirstOrDefault();

        var unreadCount = conversation.Messages.Count(m => !m.IsRead && m.SenderId != userId);

        return Task.FromResult(new ConversationDto
        {
            Id = conversation.Id,
            ListingId = conversation.ListingId,
            ListingTitle = conversation.Listing.Title,
            ListingThumbnail = thumbnail,
            OtherParticipantName = other.FullName,
            OtherParticipantId = other.Id,
            LastMessageBody = lastMessage?.Body,
            UnreadCount = unreadCount,
            LastMessageAt = conversation.LastMessageAt
        });
    }
}
