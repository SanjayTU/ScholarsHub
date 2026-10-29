using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Models.DTOs;

public class ListingSearchParams
{
    public string? Q { get; set; }
    public int? EducationLevelId { get; set; }
    public int? DepartmentId { get; set; }
    public byte? Semester { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Location { get; set; }
    public ItemCondition? Condition { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
