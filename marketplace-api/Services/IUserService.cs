using MarketplaceApi.Models.DTOs;

namespace MarketplaceApi.Services;

public interface IUserService
{
    Task<ServiceResult<UserProfileResponse>> GetPublicProfileAsync(int id);
    Task<ServiceResult<UserProfileResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<ServiceResult<object>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
