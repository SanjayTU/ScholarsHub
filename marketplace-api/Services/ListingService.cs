using MarketplaceApi.Data;
using MarketplaceApi.Helpers;
using MarketplaceApi.Models;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Services;

public class ListingService : IListingService
{
    private readonly AppDbContext _db;
    private readonly FileStorageHelper _files;
    private readonly IConfiguration _config;

    public ListingService(AppDbContext db, FileStorageHelper files, IConfiguration config)
    {
        _db = db;
        _files = files;
        _config = config;
    }

    public async Task<ServiceResult<ListingResponse>> CreateAsync(int sellerId, ListingRequest request)
    {
        var seller = await _db.Users.FirstOrDefaultAsync(u => u.Id == sellerId && !u.IsDeleted);
        if (seller is null)
            return ServiceResult<ListingResponse>.Fail(404, "User not found.");
        if (seller.Status != UserStatus.Active)
            return ServiceResult<ListingResponse>.Fail(403, "Account is not active.");

        var fieldErrors = await ValidateListingFieldsAsync(request, imagesRequired: true);
        if (fieldErrors is not null)
            return ServiceResult<ListingResponse>.Fail(400, "Validation failed.", fieldErrors);

        var (imagesOk, imageErrors) = _files.ValidateImages(request.Images, required: true);
        if (!imagesOk)
            return ServiceResult<ListingResponse>.Fail(400, "Invalid images.", imageErrors);

        List<string> savedPaths;
        try
        {
            savedPaths = await _files.SaveListingImagesAsync(request.Images);
        }
        catch (Exception)
        {
            return ServiceResult<ListingResponse>.Fail(500, "Failed to save listing images.");
        }

        var requireApproval = _config.GetValue("Features:RequireListingApproval", false);
        var now = DateTime.UtcNow;

        var listing = new Listing
        {
            SellerId = sellerId,
            CategoryId = request.CategoryId,
            EducationLevelId = request.EducationLevelId,
            DepartmentId = request.DepartmentId,
            SemesterOrClass = request.SemesterOrClass,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Condition = request.Condition,
            Location = request.Location.Trim(),
            Status = requireApproval ? ListingStatus.Unavailable : ListingStatus.Available,
            CreatedAt = now,
            UpdatedAt = now
        };

        for (var i = 0; i < savedPaths.Count; i++)
        {
            listing.Images.Add(new ListingImage
            {
                ImagePath = savedPaths[i],
                SortOrder = i
            });
        }

        _db.Listings.Add(listing);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            _files.DeleteFiles(savedPaths);
            throw;
        }

        var created = await LoadListingAsync(listing.Id);
        return ServiceResult<ListingResponse>.Ok(Map(created!), 201, "Listing created.");
    }

    public async Task<ServiceResult<ListingResponse>> UpdateAsync(int sellerId, int listingId, ListingRequest request)
    {
        var listing = await _db.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId);

        var access = CheckOwnerAccess(listing, sellerId, allowWhenRemoved: false);
        if (access is not null) return access;

        var fieldErrors = await ValidateListingFieldsAsync(request, imagesRequired: false);
        if (fieldErrors is not null)
            return ServiceResult<ListingResponse>.Fail(400, "Validation failed.", fieldErrors);

        var replaceImages = request.Images is { Count: > 0 };
        List<string>? savedPaths = null;
        List<string>? oldPaths = null;

        if (replaceImages)
        {
            var (imagesOk, imageErrors) = _files.ValidateImages(request.Images, required: true);
            if (!imagesOk)
                return ServiceResult<ListingResponse>.Fail(400, "Invalid images.", imageErrors);

            savedPaths = await _files.SaveListingImagesAsync(request.Images);
            oldPaths = listing!.Images.Select(i => i.ImagePath).ToList();
            _db.ListingImages.RemoveRange(listing.Images);
            listing.Images.Clear();

            for (var i = 0; i < savedPaths.Count; i++)
            {
                listing.Images.Add(new ListingImage
                {
                    ImagePath = savedPaths[i],
                    SortOrder = i
                });
            }
        }

        listing!.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.Price = request.Price;
        listing.Condition = request.Condition;
        listing.Location = request.Location.Trim();
        listing.CategoryId = request.CategoryId;
        listing.EducationLevelId = request.EducationLevelId;
        listing.DepartmentId = request.DepartmentId;
        listing.SemesterOrClass = request.SemesterOrClass;
        listing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            if (savedPaths is not null)
                _files.DeleteFiles(savedPaths);
            throw;
        }

        if (oldPaths is not null)
            _files.DeleteFiles(oldPaths);

        var updated = await LoadListingAsync(listing.Id);
        return ServiceResult<ListingResponse>.Ok(Map(updated!));
    }

    public async Task<ServiceResult<object>> DeleteAsync(int sellerId, int listingId)
    {
        var listing = await _db.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId);

        var access = CheckOwnerAccess(listing, sellerId, allowWhenRemoved: false);
        if (access is not null)
            return ServiceResult<object>.Fail(access.Status, access.Message, access.Errors);

        var paths = listing!.Images.Select(i => i.ImagePath).ToList();
        _db.Listings.Remove(listing);
        await _db.SaveChangesAsync();
        _files.DeleteFiles(paths);

        return ServiceResult<object>.Ok(new { message = "Listing deleted." });
    }

    public async Task<ServiceResult<ListingResponse>> ChangeStatusAsync(int sellerId, int listingId, ChangeListingStatusRequest request)
    {
        var listing = await _db.Listings.FirstOrDefaultAsync(l => l.Id == listingId);

        var access = CheckOwnerAccess(listing, sellerId, allowWhenRemoved: false);
        if (access is not null) return access;

        if (request.Status == ListingStatus.Removed)
            return ServiceResult<ListingResponse>.Fail(403, "Only an admin can set status to Removed.");

        if (listing!.Status != request.Status && !IsOwnerTransitionAllowed(listing.Status, request.Status))
            return ServiceResult<ListingResponse>.Fail(400, "Invalid status transition.",
                new Dictionary<string, string[]>
                {
                    ["Status"] = [$"Cannot change status from {listing.Status} to {request.Status}."]
                });

        if (listing.Status != request.Status)
        {
            listing.Status = request.Status;
            listing.SoldAt = request.Status == ListingStatus.Sold ? DateTime.UtcNow : null;
            listing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        var updated = await LoadListingAsync(listing.Id);
        return ServiceResult<ListingResponse>.Ok(Map(updated!));
    }

    public async Task<ServiceResult<IReadOnlyList<ListingResponse>>> GetMyListingsAsync(int sellerId)
    {
        var listings = await _db.Listings
            .AsNoTracking()
            .Include(l => l.Seller)
            .Include(l => l.Category).ThenInclude(c => c.Parent)
            .Include(l => l.EducationLevel)
            .Include(l => l.Department)
            .Include(l => l.Images)
            .Where(l => l.SellerId == sellerId)
            .OrderByDescending(l => l.UpdatedAt)
            .ToListAsync();

        IReadOnlyList<ListingResponse> data = listings.Select(Map).ToList();
        return ServiceResult<IReadOnlyList<ListingResponse>>.Ok(data);
    }

    public async Task<ServiceResult<PaginatedList<PublicListingDto>>> SearchListingsAsync(ListingSearchParams query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 12 : Math.Min(query.PageSize, 48);

        var listings = _db.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Available)
            .Where(l => !l.Seller.IsDeleted && l.Seller.Status == UserStatus.Active);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            listings = listings.Where(l => l.Title.Contains(term) || l.Description.Contains(term));
        }

        if (query.EducationLevelId.HasValue)
            listings = listings.Where(l => l.EducationLevelId == query.EducationLevelId);

        if (query.DepartmentId.HasValue)
            listings = listings.Where(l => l.DepartmentId == query.DepartmentId);

        if (query.Semester.HasValue)
            listings = listings.Where(l => l.SemesterOrClass == query.Semester);

        if (query.CategoryId.HasValue)
        {
            var categoryId = query.CategoryId.Value;
            listings = listings.Where(l =>
                l.CategoryId == categoryId || l.Category.ParentId == categoryId);
        }

        if (query.MinPrice.HasValue)
            listings = listings.Where(l => l.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            listings = listings.Where(l => l.Price <= query.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var locality = query.Location.Trim();
            listings = listings.Where(l => l.Location.Contains(locality));
        }

        if (query.Condition.HasValue)
            listings = listings.Where(l => l.Condition == query.Condition.Value);

        listings = query.Sort?.Trim().ToLowerInvariant() switch
        {
            "price" or "priceasc" or "price_asc" or "price-low" or "pricelow" or "price_low"
                => listings.OrderBy(l => l.Price).ThenByDescending(l => l.CreatedAt),
            "pricedesc" or "price_desc" or "price-high" or "pricehigh" or "price_high"
                => listings.OrderByDescending(l => l.Price).ThenByDescending(l => l.CreatedAt),
            _ => listings.OrderByDescending(l => l.CreatedAt)
        };

        var totalCount = await listings.CountAsync();
        var items = await listings
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new PublicListingDto
            {
                Id = l.Id,
                Title = l.Title,
                Price = l.Price,
                Condition = l.Condition == ItemCondition.New ? "New"
                    : l.Condition == ItemCondition.LikeNew ? "Like New"
                    : l.Condition == ItemCondition.UsedGood ? "Used - Good"
                    : l.Condition == ItemCondition.UsedFair ? "Used - Fair"
                    : l.Condition == ItemCondition.HeavilyUsed ? "Heavily Used"
                    : l.Condition.ToString(),
                Location = l.Location,
                CategoryName = l.Category.Name,
                EducationLevelName = l.EducationLevel.Name,
                CoverImageUrl = l.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => "/" + i.ImagePath)
                    .FirstOrDefault(),
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return ServiceResult<PaginatedList<PublicListingDto>>.Ok(
            PaginatedList<PublicListingDto>.Create(items, totalCount, page, pageSize));
    }

    public async Task<ServiceResult<ListingDetailDto>> GetListingDetailsAsync(int listingId)
    {
        var listing = await _db.Listings
            .AsNoTracking()
            .Include(l => l.Seller)
            .Include(l => l.Category).ThenInclude(c => c.Parent)
            .Include(l => l.EducationLevel)
            .Include(l => l.Department)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId);

        if (listing is null || listing.Status == ListingStatus.Removed)
            return ServiceResult<ListingDetailDto>.Fail(404, "Listing not found.");
        if (listing.Seller.IsDeleted || listing.Seller.Status != UserStatus.Active)
            return ServiceResult<ListingDetailDto>.Fail(404, "Listing not found.");

        var images = listing.Images
            .OrderBy(i => i.SortOrder)
            .Select(i => "/" + i.ImagePath.Replace('\\', '/'))
            .ToList();

        var dto = new ListingDetailDto
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            Condition = ConditionDisplay(listing.Condition),
            Location = listing.Location,
            Status = listing.Status.ToString(),
            CategoryId = listing.CategoryId,
            CategoryName = listing.Category?.Name ?? string.Empty,
            ParentCategoryId = listing.Category?.ParentId,
            ParentCategoryName = listing.Category?.Parent?.Name,
            EducationLevelId = listing.EducationLevelId,
            EducationLevelName = listing.EducationLevel?.Name ?? string.Empty,
            DepartmentId = listing.DepartmentId,
            DepartmentName = listing.Department?.Name,
            SemesterOrClass = listing.SemesterOrClass,
            ImageUrls = images,
            Seller = new PublicSellerDto
            {
                Id = listing.Seller.Id,
                FullName = listing.Seller.FullName,
                ProfileImagePath = listing.Seller.ProfileImagePath,
                Institution = listing.Seller.Institution,
                Locality = listing.Seller.Locality,
                ShowPhone = listing.Seller.ShowPhone
            },
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt
        };

        return ServiceResult<ListingDetailDto>.Ok(dto);
    }

    public async Task<ServiceResult<SellerContactDto>> GetSellerContactAsync(int listingId, int requesterId)
    {
        var listing = await _db.Listings
            .AsNoTracking()
            .Include(l => l.Seller)
            .FirstOrDefaultAsync(l => l.Id == listingId);

        if (listing is null || listing.Status == ListingStatus.Removed)
            return ServiceResult<SellerContactDto>.Fail(404, "Listing not found.");
        if (listing.Seller.IsDeleted || listing.Seller.Status != UserStatus.Active)
            return ServiceResult<SellerContactDto>.Fail(404, "Listing not found.");
        if (listing.Status != ListingStatus.Available)
            return ServiceResult<SellerContactDto>.Fail(400, "This listing is not available.");
        if (!listing.Seller.ShowPhone)
            return ServiceResult<SellerContactDto>.Fail(403, "Seller has hidden their phone number.");

        _ = requesterId;

        return ServiceResult<SellerContactDto>.Ok(new SellerContactDto
        {
            ListingId = listing.Id,
            SellerId = listing.Seller.Id,
            FullName = listing.Seller.FullName,
            Mobile = listing.Seller.Mobile
        });
    }

    private async Task<Dictionary<string, string[]>?> ValidateListingFieldsAsync(ListingRequest request, bool imagesRequired)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length < 5 || request.Title.Trim().Length > 120)
            errors["Title"] = ["Title must be 5–120 characters."];

        if (request.Description is { Length: > 2000 })
            errors["Description"] = ["Description must be at most 2000 characters."];

        if (request.Price < 0 || request.Price > 100000)
            errors["Price"] = ["Price must be between 0 and 100000."];

        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive))
            errors["CategoryId"] = ["Invalid category."];

        var educationLevel = await _db.EducationLevels.FirstOrDefaultAsync(e => e.Id == request.EducationLevelId && e.IsActive);
        if (educationLevel is null)
            errors["EducationLevelId"] = ["Invalid education level."];

        if (request.DepartmentId.HasValue &&
            !await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId && d.IsActive))
            errors["DepartmentId"] = ["Invalid department."];

        if (request.SemesterOrClass.HasValue)
        {
            var max = educationLevel?.Type == EducationLevelType.College ? (byte)8 : (byte)12;
            if (request.SemesterOrClass < 1 || request.SemesterOrClass > max)
                errors["SemesterOrClass"] = [educationLevel?.Type == EducationLevelType.College
                    ? "Semester must be between 1 and 8."
                    : "Class must be between 1 and 12."];
        }

        if (!Enum.IsDefined(request.Condition))
            errors["Condition"] = ["Invalid condition."];

        if (imagesRequired && (request.Images is null || request.Images.Count == 0))
            errors["Images"] = ["At least 1 image is required."];

        return errors.Count == 0 ? null : errors;
    }

    private static bool IsOwnerTransitionAllowed(ListingStatus current, ListingStatus next) =>
        (current, next) switch
        {
            (ListingStatus.Available, ListingStatus.Sold) => true,
            (ListingStatus.Available, ListingStatus.Unavailable) => true,
            (ListingStatus.Sold, ListingStatus.Available) => true,
            (ListingStatus.Unavailable, ListingStatus.Available) => true,
            _ => false
        };

    private static ServiceResult<ListingResponse>? CheckOwnerAccess(Listing? listing, int sellerId, bool allowWhenRemoved)
    {
        if (listing is null)
            return ServiceResult<ListingResponse>.Fail(404, "Listing not found.");
        if (listing.SellerId != sellerId)
            return ServiceResult<ListingResponse>.Fail(403, "You can only manage your own listings.");
        if (!allowWhenRemoved && listing.Status == ListingStatus.Removed)
            return ServiceResult<ListingResponse>.Fail(403, "Removed listings cannot be changed by the owner.");
        return null;
    }

    private Task<Listing?> LoadListingAsync(int id) =>
        _db.Listings
            .AsNoTracking()
            .Include(l => l.Seller)
            .Include(l => l.Category).ThenInclude(c => c.Parent)
            .Include(l => l.EducationLevel)
            .Include(l => l.Department)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id);

    private static ListingResponse Map(Listing listing)
    {
        var images = listing.Images
            .OrderBy(i => i.SortOrder)
            .Select(i => "/" + i.ImagePath.Replace('\\', '/'))
            .ToList();

        return new ListingResponse
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            Condition = ConditionDisplay(listing.Condition),
            Location = listing.Location,
            Status = listing.Status.ToString(),
            CategoryId = listing.CategoryId,
            CategoryName = listing.Category?.Name ?? string.Empty,
            ParentCategoryId = listing.Category?.ParentId,
            ParentCategoryName = listing.Category?.Parent?.Name,
            EducationLevelId = listing.EducationLevelId,
            EducationLevelName = listing.EducationLevel?.Name ?? string.Empty,
            DepartmentId = listing.DepartmentId,
            DepartmentName = listing.Department?.Name,
            SemesterOrClass = listing.SemesterOrClass,
            ImageUrls = images,
            Seller = new ListingSellerDto
            {
                Id = listing.Seller.Id,
                FullName = listing.Seller.FullName,
                ProfileImagePath = listing.Seller.ProfileImagePath,
                Institution = listing.Seller.Institution,
                Locality = listing.Seller.Locality,
                ShowPhone = listing.Seller.ShowPhone,
                Mobile = listing.Seller.ShowPhone ? listing.Seller.Mobile : null
            },
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            SoldAt = listing.SoldAt
        };
    }

    private static string ConditionDisplay(ItemCondition condition) => condition switch
    {
        ItemCondition.New => "New",
        ItemCondition.LikeNew => "Like New",
        ItemCondition.UsedGood => "Used - Good",
        ItemCondition.UsedFair => "Used - Fair",
        ItemCondition.HeavilyUsed => "Heavily Used",
        _ => condition.ToString()
    };
}
