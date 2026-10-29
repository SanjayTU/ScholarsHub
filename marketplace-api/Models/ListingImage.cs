namespace MarketplaceApi.Models;

/// <summary>
/// Image associated with a listing. Cascade-deleted when the listing is deleted.
/// </summary>
public class ListingImage
{
    public int Id { get; set; }

    public int ListingId { get; set; }

    public string ImagePath { get; set; } = string.Empty;

    /// <summary>Display order; 0 = cover image.</summary>
    public int SortOrder { get; set; }

    // Navigation
    public Listing Listing { get; set; } = null!;
}
