using MarketplaceApi.Models.DTOs;

namespace MarketplaceApi.Services;

public interface IReportService
{
    Task<ServiceResult<ReportDto>> CreateReportAsync(int reporterId, int listingId, ReportRequest request);
}
