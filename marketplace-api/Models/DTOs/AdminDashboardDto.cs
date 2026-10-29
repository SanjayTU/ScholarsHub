namespace MarketplaceApi.Models.DTOs;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveListings { get; set; }
    public int SoldItems { get; set; }
    public int PendingReports { get; set; }
    public int BlockedUsers { get; set; }
}
