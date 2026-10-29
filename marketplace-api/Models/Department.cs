namespace MarketplaceApi.Models;

/// <summary>
/// Academic departments (e.g. Computer Applications, Commerce, Physics).
/// </summary>
public class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
