namespace MarketplaceApi.Models.DTOs;

public class PublicSellerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Locality { get; set; } = string.Empty;
    public bool ShowPhone { get; set; }
}

public class ListingDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int EducationLevelId { get; set; }
    public string EducationLevelName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public byte? SemesterOrClass { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public PublicSellerDto Seller { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
