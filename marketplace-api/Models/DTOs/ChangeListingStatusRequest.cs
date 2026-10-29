using System.ComponentModel.DataAnnotations;
using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Models.DTOs;

public class ChangeListingStatusRequest
{
    [Required]
    public ListingStatus Status { get; set; }
}
