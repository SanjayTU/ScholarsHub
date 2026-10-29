using MarketplaceApi.Models.DTOs;

namespace MarketplaceApi.Services;

public interface IChatService
{
    Task<ServiceResult<ConversationDto>> StartOrGetConversationAsync(int userId, StartChatRequest request);
    Task<ServiceResult<IReadOnlyList<ConversationDto>>> GetInboxAsync(int userId);
    Task<ServiceResult<IReadOnlyList<MessageDto>>> GetMessagesAsync(int userId, int conversationId, int? after = null);
    Task<ServiceResult<MessageDto>> SendMessageAsync(int userId, int conversationId, SendMessageRequest request);
    Task<ServiceResult<object>> GetUnreadCountAsync(int userId);
}
