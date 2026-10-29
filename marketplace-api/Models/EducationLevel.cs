using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Models;

/// <summary>
/// Education levels: Primary, High School, Higher Secondary, Undergraduate, Postgraduate.
/// </summary>
public class EducationLevel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>School or College classification.</summary>
    public EducationLevelType Type { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
