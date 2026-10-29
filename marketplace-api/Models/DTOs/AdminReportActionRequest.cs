using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.DTOs;

public class AdminReportActionRequest
{
    [Required]
    public string Action { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AdminNote { get; set; }
}
