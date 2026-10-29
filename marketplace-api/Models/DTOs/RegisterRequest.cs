using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.DTOs;

public class RegisterRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile must be a 10-digit Indian number starting with 6–9.")]
    public string Mobile { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must be at least 8 characters and contain at least one letter and one digit.")]
    public string Password { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int EducationLevelId { get; set; }

    [Required, StringLength(150, MinimumLength = 2)]
    public string Institution { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    [Range(1, 12)]
    public byte SemesterOrClass { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Locality { get; set; } = string.Empty;
}
