using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.DTOs;

public class SendMessageRequest
{
    [Required, StringLength(1000, MinimumLength = 1)]
    public string Body { get; set; } = string.Empty;
}
