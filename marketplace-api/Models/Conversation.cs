namespace MarketplaceApi.Models;

/// <summary>
/// A chat conversation between a buyer and seller about a specific listing.
/// Unique constraint on (ListingId, BuyerId).
/// </summary>
public class Conversation
{
    public int Id { get; set; }

    public int ListingId { get; set; }

    public int BuyerId { get; set; }

    public int SellerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastMessageAt { get; set; }

    // Navigation properties
    public Listing Listing { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
