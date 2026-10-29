using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.DTOs;

public class StartChatRequest
{
    [Required, Range(1, int.MaxValue)]
    public int ListingId { get; set; }
}
