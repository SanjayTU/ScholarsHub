using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Services;

public interface IAdminService
{
    Task<ServiceResult<AdminDashboardDto>> GetDashboardStatsAsync();
    Task<ServiceResult<PaginatedList<AdminUserDto>>> GetUsersAsync(string? q, int page, int pageSize);
    Task<ServiceResult<AdminUserDto>> ToggleUserStatusAsync(int adminId, int userId);
    Task<ServiceResult<object>> DeleteUserAsync(int adminId, int userId);
    Task<ServiceResult<PaginatedList<AdminListingDto>>> GetListingsAsync(string? q, ListingStatus? status, int page, int pageSize);
    Task<ServiceResult<object>> RemoveListingAsync(int listingId);
    Task<ServiceResult<IReadOnlyList<AdminCategoryDto>>> GetCategoriesAsync();
    Task<ServiceResult<AdminCategoryDto>> AddCategoryAsync(AdminCategoryRequest request);
    Task<ServiceResult<AdminCategoryDto>> UpdateCategoryAsync(int id, AdminCategoryRequest request);
    Task<ServiceResult<AdminCategoryDto>> ToggleCategoryStatusAsync(int id);
    Task<ServiceResult<IReadOnlyList<ReportDto>>> GetReportsAsync(ReportStatus? status);
    Task<ServiceResult<ReportDto>> HandleReportAsync(int adminId, int reportId, AdminReportActionRequest request);
}
