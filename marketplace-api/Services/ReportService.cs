using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.DTOs;
using MarketplaceApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    public async Task<ServiceResult<ReportDto>> CreateReportAsync(int reporterId, int listingId, ReportRequest request)
    {
        var reporter = await _db.Users.FirstOrDefaultAsync(u => u.Id == reporterId && !u.IsDeleted);
        if (reporter is null)
            return ServiceResult<ReportDto>.Fail(404, "User not found.");
        if (reporter.Status != UserStatus.Active)
            return ServiceResult<ReportDto>.Fail(403, "Account is not active.");

        var listing = await _db.Listings.FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing is null)
            return ServiceResult<ReportDto>.Fail(404, "Listing not found.");

        if (listing.SellerId == reporterId)
            return ServiceResult<ReportDto>.Fail(403, "You cannot report your own listing.");

        if (!TryParseReason(request.Reason, out var reason))
            return ServiceResult<ReportDto>.Fail(400, "Invalid report reason.",
                new Dictionary<string, string[]> { ["Reason"] = ["Reason must be Misleading, Prohibited Item, Fraud/Scam, Wrong Category, or Other."] });

        var duplicate = await _db.Reports.AnyAsync(r => r.ListingId == listingId && r.ReporterId == reporterId);
        if (duplicate)
            return ServiceResult<ReportDto>.Fail(409, "You have already reported this listing.");

        var report = new Report
        {
            ListingId = listingId,
            ReporterId = reporterId,
            Reason = reason,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.Reports.Add(report);
        await _db.SaveChangesAsync();

        return ServiceResult<ReportDto>.Ok(new ReportDto
        {
            Id = report.Id,
            ListingId = listing.Id,
            ListingTitle = listing.Title,
            ReporterId = reporter.Id,
            ReporterName = reporter.FullName,
            Reason = report.Reason.ToString(),
            Comment = report.Comment,
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt
        }, 201);
    }

    internal static bool TryParseReason(string? value, out ReportReason reason)
    {
        reason = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var key = value.Trim().Replace(" ", "").Replace("-", "").Replace("/", "");
        return key.ToLowerInvariant() switch
        {
            "misleading" => Assign(ReportReason.Misleading, out reason),
            "prohibiteditem" => Assign(ReportReason.ProhibitedItem, out reason),
            "fraudscam" => Assign(ReportReason.FraudScam, out reason),
            "wrongcategory" => Assign(ReportReason.WrongCategory, out reason),
            "other" => Assign(ReportReason.Other, out reason),
            _ => Enum.TryParse(value, true, out reason)
        };
    }

    private static bool Assign(ReportReason value, out ReportReason reason)
    {
        reason = value;
        return true;
    }
}
