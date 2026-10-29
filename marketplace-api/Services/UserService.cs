using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public async Task<ServiceResult<UserProfileResponse>> GetPublicProfileAsync(int id)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.EducationLevel)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user is null)
            return ServiceResult<UserProfileResponse>.Fail(404, "User not found.");

        var profile = await MapProfileAsync(_db, user, includePrivate: false);
        return ServiceResult<UserProfileResponse>.Ok(profile);
    }

    public async Task<ServiceResult<UserProfileResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _db.Users
            .Include(u => u.EducationLevel)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user is null)
            return ServiceResult<UserProfileResponse>.Fail(404, "User not found.");

        if (!await _db.EducationLevels.AnyAsync(e => e.Id == request.EducationLevelId && e.IsActive))
            return ServiceResult<UserProfileResponse>.Fail(400, "Invalid education level.",
                new Dictionary<string, string[]> { ["EducationLevelId"] = ["Invalid education level."] });

        if (request.DepartmentId.HasValue &&
            !await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId && d.IsActive))
            return ServiceResult<UserProfileResponse>.Fail(400, "Invalid department.",
                new Dictionary<string, string[]> { ["DepartmentId"] = ["Invalid department."] });

        user.FullName = request.FullName.Trim();
        user.EducationLevelId = request.EducationLevelId;
        user.Institution = request.Institution.Trim();
        user.DepartmentId = request.DepartmentId;
        user.SemesterOrClass = request.SemesterOrClass;
        user.Locality = request.Locality.Trim();
        user.ShowPhone = request.ShowPhone;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _db.Entry(user).Reference(u => u.EducationLevel).LoadAsync();
        await _db.Entry(user).Reference(u => u.Department).LoadAsync();

        var profile = await MapProfileAsync(_db, user, includePrivate: true);
        return ServiceResult<UserProfileResponse>.Ok(profile);
    }

    public async Task<ServiceResult<object>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user is null)
            return ServiceResult<object>.Fail(404, "User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ServiceResult<object>.Fail(400, "Current password is incorrect.",
                new Dictionary<string, string[]> { ["CurrentPassword"] = ["Current password is incorrect."] });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ServiceResult<object>.Ok(new { message = "Password changed successfully." }, 200, "Password changed successfully.");
    }

    internal static async Task<UserProfileResponse> MapProfileAsync(AppDbContext db, User user, bool includePrivate)
    {
        var activeCount = await db.Listings.CountAsync(l =>
            l.SellerId == user.Id && l.Status == ListingStatus.Available);
        var soldCount = await db.Listings.CountAsync(l =>
            l.SellerId == user.Id && l.Status == ListingStatus.Sold);

        return new UserProfileResponse
        {
            Id = user.Id,
            ProfileImagePath = user.ProfileImagePath,
            FullName = user.FullName,
            EducationLevelId = user.EducationLevelId,
            EducationLevelName = user.EducationLevel?.Name ?? string.Empty,
            Institution = user.Institution,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.Name,
            SemesterOrClass = user.SemesterOrClass,
            Locality = user.Locality,
            ActiveListingsCount = activeCount,
            SoldListingsCount = soldCount,
            MemberSince = user.CreatedAt,
            ShowPhone = user.ShowPhone,
            Email = includePrivate ? user.Email : null,
            Mobile = includePrivate ? user.Mobile : null,
            Role = includePrivate ? user.Role.ToString() : null
        };
    }
}
