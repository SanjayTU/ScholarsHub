using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Models;

/// <summary>
/// A report filed by a user against a listing.
/// Unique constraint on (ListingId, ReporterId).
/// </summary>
public class Report
{
    public int Id { get; set; }

    public int ListingId { get; set; }

    public int ReporterId { get; set; }

    public ReportReason Reason { get; set; }

    public string? Comment { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public Listing Listing { get; set; } = null!;
    public User Reporter { get; set; } = null!;
}
