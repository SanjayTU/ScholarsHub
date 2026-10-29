using System.Text.RegularExpressions;
using MarketplaceApi.Data;
using MarketplaceApi.Helpers;
using MarketplaceApi.Models;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Services;

public class AuthService : IAuthService
{
    private static readonly Regex MobileRegex = new(@"^[6-9]\d{9}$", RegexOptions.Compiled);

    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;

    public AuthService(AppDbContext db, JwtHelper jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var mobile = request.Mobile.Trim();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return ServiceResult<AuthResponse>.Fail(409, "Email is already registered.",
                new Dictionary<string, string[]> { ["Email"] = ["Email is already registered."] });

        if (await _db.Users.AnyAsync(u => u.Mobile == mobile))
            return ServiceResult<AuthResponse>.Fail(409, "Mobile number is already registered.",
                new Dictionary<string, string[]> { ["Mobile"] = ["Mobile number is already registered."] });

        if (!await _db.EducationLevels.AnyAsync(e => e.Id == request.EducationLevelId && e.IsActive))
            return ServiceResult<AuthResponse>.Fail(400, "Invalid education level.",
                new Dictionary<string, string[]> { ["EducationLevelId"] = ["Invalid education level."] });

        if (request.DepartmentId.HasValue &&
            !await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId && d.IsActive))
            return ServiceResult<AuthResponse>.Fail(400, "Invalid department.",
                new Dictionary<string, string[]> { ["DepartmentId"] = ["Invalid department."] });

        var now = DateTime.UtcNow;
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Mobile = mobile,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EducationLevelId = request.EducationLevelId,
            DepartmentId = request.DepartmentId,
            Institution = request.Institution.Trim(),
            SemesterOrClass = request.SemesterOrClass,
            Locality = request.Locality.Trim(),
            ShowPhone = true,
            Role = Role.Student,
            Status = UserStatus.Active,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return ServiceResult<AuthResponse>.Ok(BuildAuthResponse(user), 201, "Registration successful.");
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var identifier = request.EmailOrMobile.Trim();
        User? user;

        if (MobileRegex.IsMatch(identifier))
            user = await _db.Users.FirstOrDefaultAsync(u => u.Mobile == identifier && !u.IsDeleted);
        else
        {
            var email = identifier.ToLowerInvariant();
            user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ServiceResult<AuthResponse>.Fail(401, "Invalid email/mobile or password.");

        if (user.Status == UserStatus.Disabled)
            return ServiceResult<AuthResponse>.Fail(403, "This account has been disabled. Contact support.");

        return ServiceResult<AuthResponse>.Ok(BuildAuthResponse(user));
    }

    public async Task<ServiceResult<UserProfileResponse>> GetMeAsync(int userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.EducationLevel)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user is null)
            return ServiceResult<UserProfileResponse>.Fail(404, "User not found.");

        var profile = await UserService.MapProfileAsync(_db, user, includePrivate: true);
        return ServiceResult<UserProfileResponse>.Ok(profile);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var (token, expiresAt) = _jwt.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
