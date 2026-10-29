using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Models;

/// <summary>
/// A product listing posted by a student seller.
/// </summary>
public class Listing
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public int CategoryId { get; set; }

    public int EducationLevelId { get; set; }

    public int? DepartmentId { get; set; }

    /// <summary>Semester (college) or class (school). Nullable.</summary>
    public byte? SemesterOrClass { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ItemCondition Condition { get; set; }

    public string Location { get; set; } = string.Empty;

    public ListingStatus Status { get; set; } = ListingStatus.Available;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? SoldAt { get; set; }

    // Navigation properties
    public User Seller { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public EducationLevel EducationLevel { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
