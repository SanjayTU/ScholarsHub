using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.DTOs;

public class ReportRequest
{
    [Required]
    public string Reason { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Comment { get; set; }
}
