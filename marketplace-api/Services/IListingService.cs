using MarketplaceApi.Models.DTOs;

namespace MarketplaceApi.Services;

public interface IListingService
{
    Task<ServiceResult<ListingResponse>> CreateAsync(int sellerId, ListingRequest request);
    Task<ServiceResult<ListingResponse>> UpdateAsync(int sellerId, int listingId, ListingRequest request);
    Task<ServiceResult<object>> DeleteAsync(int sellerId, int listingId);
    Task<ServiceResult<ListingResponse>> ChangeStatusAsync(int sellerId, int listingId, ChangeListingStatusRequest request);
    Task<ServiceResult<IReadOnlyList<ListingResponse>>> GetMyListingsAsync(int sellerId);
    Task<ServiceResult<PaginatedList<PublicListingDto>>> SearchListingsAsync(ListingSearchParams query);
    Task<ServiceResult<ListingDetailDto>> GetListingDetailsAsync(int listingId);
    Task<ServiceResult<SellerContactDto>> GetSellerContactAsync(int listingId, int requesterId);
}
