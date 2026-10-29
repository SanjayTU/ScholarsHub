namespace MarketplaceApi.Models.DTOs;

public class UserProfileResponse
{
    public int Id { get; set; }
    public string? ProfileImagePath { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int EducationLevelId { get; set; }
    public string EducationLevelName { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public byte SemesterOrClass { get; set; }
    public string Locality { get; set; } = string.Empty;
    public int ActiveListingsCount { get; set; }
    public int SoldListingsCount { get; set; }
    public DateTime MemberSince { get; set; }
    public bool ShowPhone { get; set; }

    /// <summary>Populated only for GET /api/auth/me — never on public profiles.</summary>
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Role { get; set; }
}
