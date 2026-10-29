using System.ComponentModel.DataAnnotations;
using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Models.DTOs;

public class ListingRequest
{
    [Required, StringLength(120, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int EducationLevelId { get; set; }

    public int? DepartmentId { get; set; }

    [Range(1, 12)]
    public byte? SemesterOrClass { get; set; }

    [Required]
    public ItemCondition Condition { get; set; }

    [Required, Range(0, 100000)]
    public decimal Price { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public List<IFormFile> Images { get; set; } = new();
}
