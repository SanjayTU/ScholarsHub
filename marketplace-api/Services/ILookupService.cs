using MarketplaceApi.Models.DTOs;

namespace MarketplaceApi.Services;

public interface ILookupService
{
    Task<IReadOnlyList<LookupItemDto>> GetEducationLevelsAsync();
    Task<IReadOnlyList<LookupItemDto>> GetDepartmentsAsync();
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync();
    IReadOnlyList<LookupItemDto> GetConditions();
}
