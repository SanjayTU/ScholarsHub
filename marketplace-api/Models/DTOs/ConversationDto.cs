namespace MarketplaceApi.Models.DTOs;

public class ConversationDto
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public string? ListingThumbnail { get; set; }
    public string OtherParticipantName { get; set; } = string.Empty;
    public int OtherParticipantId { get; set; }
    public string? LastMessageBody { get; set; }
    public int UnreadCount { get; set; }
    public DateTime LastMessageAt { get; set; }
}
