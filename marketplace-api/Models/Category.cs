namespace MarketplaceApi.Models;

/// <summary>
/// Single-table category hierarchy. ParentId = null means top-level group.
/// </summary>
public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Null for top-level groups; set for sub-categories.</summary>
    public int? ParentId { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
