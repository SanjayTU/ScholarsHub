using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.DTOs;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8), RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must be at least 8 characters and contain at least one letter and one digit.")]
    public string NewPassword { get; set; } = string.Empty;
}
