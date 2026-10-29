using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.DTOs;

public class LoginRequest
{
    [Required]
    public string EmailOrMobile { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
