using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db) => _db = db;

    public async Task<ServiceResult<AdminDashboardDto>> GetDashboardStatsAsync()
    {
        var dto = new AdminDashboardDto
        {
            TotalUsers = await _db.Users.CountAsync(u => !u.IsDeleted),
            ActiveListings = await _db.Listings.CountAsync(l => l.Status == ListingStatus.Available),
            SoldItems = await _db.Listings.CountAsync(l => l.Status == ListingStatus.Sold),
            PendingReports = await _db.Reports.CountAsync(r => r.Status == ReportStatus.Pending),
            BlockedUsers = await _db.Users.CountAsync(u => !u.IsDeleted && u.Status == UserStatus.Disabled)
        };
        return ServiceResult<AdminDashboardDto>.Ok(dto);
    }

    public async Task<ServiceResult<PaginatedList<AdminUserDto>>> GetUsersAsync(string? q, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 12 : Math.Min(pageSize, 48);

        var query = _db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u =>
                u.FullName.Contains(term) ||
                u.Email.Contains(term) ||
                u.Mobile.Contains(term));
        }

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = users.Select(MapUser).ToList();
        return ServiceResult<PaginatedList<AdminUserDto>>.Ok(PaginatedList<AdminUserDto>.Create(items, total, page, pageSize));
    }

    public async Task<ServiceResult<AdminUserDto>> ToggleUserStatusAsync(int adminId, int userId)
    {
        if (adminId == userId)
            return ServiceResult<AdminUserDto>.Fail(403, "You cannot disable or enable your own account.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user is null)
            return ServiceResult<AdminUserDto>.Fail(404, "User not found.");

        user.Status = user.Status == UserStatus.Active ? UserStatus.Disabled : UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ServiceResult<AdminUserDto>.Ok(MapUser(user));
    }

    public async Task<ServiceResult<object>> DeleteUserAsync(int adminId, int userId)
    {
        if (adminId == userId)
            return ServiceResult<object>.Fail(403, "You cannot delete your own account.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user is null)
            return ServiceResult<object>.Fail(404, "User not found.");

        user.IsDeleted = true;
        user.Status = UserStatus.Disabled;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ServiceResult<object>.Ok(new { message = "User deleted." });
    }

    public async Task<ServiceResult<PaginatedList<AdminListingDto>>> GetListingsAsync(string? q, ListingStatus? status, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 12 : Math.Min(pageSize, 48);

        var query = _db.Listings
            .AsNoTracking()
            .Include(l => l.Seller)
            .Include(l => l.Category)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(l => l.Title.Contains(term) || l.Seller.FullName.Contains(term));
        }

        var total = await query.CountAsync();
        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = listings.Select(MapListing).ToList();
        return ServiceResult<PaginatedList<AdminListingDto>>.Ok(PaginatedList<AdminListingDto>.Create(items, total, page, pageSize));
    }

    public async Task<ServiceResult<object>> RemoveListingAsync(int listingId)
    {
        var listing = await _db.Listings.FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing is null)
            return ServiceResult<object>.Fail(404, "Listing not found.");

        listing.Status = ListingStatus.Removed;
        listing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ServiceResult<object>.Ok(new { message = "Listing removed." });
    }

    public async Task<ServiceResult<IReadOnlyList<AdminCategoryDto>>> GetCategoriesAsync()
    {
        var items = await _db.Categories
            .AsNoTracking()
            .Include(c => c.Parent)
            .Include(c => c.Listings)
            .OrderBy(c => c.Name)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                IsActive = c.IsActive,
                ListingCount = c.Listings.Count
            })
            .ToListAsync();

        return ServiceResult<IReadOnlyList<AdminCategoryDto>>.Ok(items);
    }

    public async Task<ServiceResult<AdminCategoryDto>> AddCategoryAsync(AdminCategoryRequest request)
    {
        var name = request.Name.Trim();
        if (request.ParentId.HasValue)
        {
            var parent = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.ParentId.Value);
            if (parent is null)
                return ServiceResult<AdminCategoryDto>.Fail(400, "Parent category not found.",
                    new Dictionary<string, string[]> { ["ParentId"] = ["Invalid parent category."] });
        }

        var category = new Category
        {
            Name = name,
            ParentId = request.ParentId,
            IsActive = true
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return ServiceResult<AdminCategoryDto>.Ok(await LoadCategoryAsync(category.Id), 201);
    }

    public async Task<ServiceResult<AdminCategoryDto>> UpdateCategoryAsync(int id, AdminCategoryRequest request)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return ServiceResult<AdminCategoryDto>.Fail(404, "Category not found.");

        if (request.ParentId == id)
            return ServiceResult<AdminCategoryDto>.Fail(400, "A category cannot be its own parent.");

        if (request.ParentId.HasValue)
        {
            var parent = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.ParentId.Value);
            if (parent is null)
                return ServiceResult<AdminCategoryDto>.Fail(400, "Parent category not found.",
                    new Dictionary<string, string[]> { ["ParentId"] = ["Invalid parent category."] });
        }

        category.Name = request.Name.Trim();
        category.ParentId = request.ParentId;
        await _db.SaveChangesAsync();
        return ServiceResult<AdminCategoryDto>.Ok(await LoadCategoryAsync(category.Id));
    }

    public async Task<ServiceResult<AdminCategoryDto>> ToggleCategoryStatusAsync(int id)
    {
        var category = await _db.Categories.Include(c => c.Listings).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return ServiceResult<AdminCategoryDto>.Fail(404, "Category not found.");

        if (category.IsActive && category.Listings.Count > 0)
        {
            category.IsActive = false;
            await _db.SaveChangesAsync();
            return ServiceResult<AdminCategoryDto>.Ok(await LoadCategoryAsync(category.Id));
        }

        category.IsActive = !category.IsActive;
        await _db.SaveChangesAsync();
        return ServiceResult<AdminCategoryDto>.Ok(await LoadCategoryAsync(category.Id));
    }

    public async Task<ServiceResult<IReadOnlyList<ReportDto>>> GetReportsAsync(ReportStatus? status)
    {
        var query = _db.Reports
            .AsNoTracking()
            .Include(r => r.Listing)
            .Include(r => r.Reporter)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        IReadOnlyList<ReportDto> items = reports.Select(MapReport).ToList();
        return ServiceResult<IReadOnlyList<ReportDto>>.Ok(items);
    }

    public async Task<ServiceResult<ReportDto>> HandleReportAsync(int adminId, int reportId, AdminReportActionRequest request)
    {
        var report = await _db.Reports
            .Include(r => r.Listing)
            .Include(r => r.Reporter)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report is null)
            return ServiceResult<ReportDto>.Fail(404, "Report not found.");

        if (report.Status != ReportStatus.Pending)
            return ServiceResult<ReportDto>.Fail(400, "This report has already been handled.");

        var action = (request.Action ?? string.Empty).Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;
        report.AdminNote = string.IsNullOrWhiteSpace(request.AdminNote) ? null : request.AdminNote.Trim();
        report.ResolvedAt = now;

        switch (action)
        {
            case "dismiss":
                report.Status = ReportStatus.Dismissed;
                break;
            case "removelisting":
                report.Listing.Status = ListingStatus.Removed;
                report.Listing.UpdatedAt = now;
                report.Status = ReportStatus.Resolved;
                break;
            case "disableseller":
                var seller = await _db.Users.FirstOrDefaultAsync(u => u.Id == report.Listing.SellerId && !u.IsDeleted);
                if (seller is null)
                    return ServiceResult<ReportDto>.Fail(404, "Seller not found.");
                if (seller.Id == adminId)
                    return ServiceResult<ReportDto>.Fail(403, "You cannot disable your own account.");
                seller.Status = UserStatus.Disabled;
                seller.UpdatedAt = now;
                report.Status = ReportStatus.Resolved;
                break;
            default:
                return ServiceResult<ReportDto>.Fail(400, "Invalid action.",
                    new Dictionary<string, string[]> { ["Action"] = ["Action must be dismiss, removeListing, or disableSeller."] });
        }

        await _db.SaveChangesAsync();
        return ServiceResult<ReportDto>.Ok(MapReport(report));
    }

    private async Task<AdminCategoryDto> LoadCategoryAsync(int id) =>
        await _db.Categories
            .AsNoTracking()
            .Include(c => c.Parent)
            .Where(c => c.Id == id)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                IsActive = c.IsActive,
                ListingCount = c.Listings.Count
            })
            .FirstAsync();

    private static AdminUserDto MapUser(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Mobile = u.Mobile,
        Role = u.Role.ToString(),
        Status = u.Status.ToString(),
        IsDeleted = u.IsDeleted,
        CreatedAt = u.CreatedAt
    };

    private static AdminListingDto MapListing(Listing l) => new()
    {
        Id = l.Id,
        Title = l.Title,
        Price = l.Price,
        Status = l.Status.ToString(),
        CategoryName = l.Category?.Name ?? string.Empty,
        SellerId = l.SellerId,
        SellerName = l.Seller.FullName,
        CreatedAt = l.CreatedAt
    };

    private static ReportDto MapReport(Report r) => new()
    {
        Id = r.Id,
        ListingId = r.ListingId,
        ListingTitle = r.Listing.Title,
        ReporterId = r.ReporterId,
        ReporterName = r.Reporter.FullName,
        Reason = r.Reason.ToString(),
        Comment = r.Comment,
        Status = r.Status.ToString(),
        CreatedAt = r.CreatedAt
    };
}
