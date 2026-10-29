using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.DTOs;

public class UpdateProfileRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int EducationLevelId { get; set; }

    [Required, StringLength(150, MinimumLength = 2)]
    public string Institution { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    [Range(1, 12)]
    public byte SemesterOrClass { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Locality { get; set; } = string.Empty;

    public bool ShowPhone { get; set; } = true;
}
