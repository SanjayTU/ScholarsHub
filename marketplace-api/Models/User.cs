using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Models;

/// <summary>
/// Represents a registered user (student or admin).
/// </summary>
public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int EducationLevelId { get; set; }

    public int? DepartmentId { get; set; }

    public string Institution { get; set; } = string.Empty;

    /// <summary>1–12 for school classes, 1–8 for college semesters.</summary>
    public byte SemesterOrClass { get; set; }

    public string Locality { get; set; } = string.Empty;

    public string? ProfileImagePath { get; set; }

    /// <summary>Whether the seller's phone number is visible to logged-in users.</summary>
    public bool ShowPhone { get; set; } = true;

    public Role Role { get; set; } = Role.Student;

    public UserStatus Status { get; set; } = UserStatus.Active;

    /// <summary>Soft-delete flag.</summary>
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public EducationLevel EducationLevel { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Conversation> BuyerConversations { get; set; } = new List<Conversation>();
    public ICollection<Conversation> SellerConversations { get; set; } = new List<Conversation>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
