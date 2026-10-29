using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.DTOs;

public class AdminCategoryRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }
}
