using MarketplaceApi.Data;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Services;

public class LookupService : ILookupService
{
    private readonly AppDbContext _db;

    public LookupService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<LookupItemDto>> GetEducationLevelsAsync()
    {
        return await _db.EducationLevels
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Id)
            .Select(e => new LookupItemDto { Id = e.Id, Name = e.Name })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetDepartmentsAsync()
    {
        return await _db.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new LookupItemDto { Id = d.Id, Name = d.Name })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId
            })
            .ToListAsync();

        var lookup = categories.ToDictionary(c => c.Id);
        var roots = new List<CategoryDto>();

        foreach (var category in categories)
        {
            if (category.ParentId is int parentId && lookup.TryGetValue(parentId, out var parent))
                parent.Children.Add(category);
            else
                roots.Add(category);
        }

        return roots;
    }

    public IReadOnlyList<LookupItemDto> GetConditions() =>
    [
        new() { Id = (int)ItemCondition.New, Name = "New" },
        new() { Id = (int)ItemCondition.LikeNew, Name = "Like New" },
        new() { Id = (int)ItemCondition.UsedGood, Name = "Used - Good" },
        new() { Id = (int)ItemCondition.UsedFair, Name = "Used - Fair" },
        new() { Id = (int)ItemCondition.HeavilyUsed, Name = "Heavily Used" }
    ];
}
