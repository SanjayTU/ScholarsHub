namespace MarketplaceApi.Models.DTOs;

public class SellerContactDto
{
    public int ListingId { get; set; }
    public int SellerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
}
