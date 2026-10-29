using Microsoft.EntityFrameworkCore;
using MarketplaceApi.Models;
using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Data;

/// <summary>
/// Application database context — EF Core Code-First with SQLite.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<EducationLevel> EducationLevels => Set<EducationLevel>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ───────────────────────── User ─────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Mobile).HasMaxLength(10).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Institution).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Locality).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ProfileImagePath).HasMaxLength(255);
            entity.Property(e => e.ShowPhone).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            // Enum → string storage
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(Role.Student);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(UserStatus.Active);

            // Unique constraints / indexes
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Mobile).IsUnique();

            // Relationships
            entity.HasOne(e => e.EducationLevel)
                .WithMany(el => el.Users)
                .HasForeignKey(e => e.EducationLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ───────────────────── EducationLevel ─────────────────────
        modelBuilder.Entity<EducationLevel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        // ───────────────────── Department ─────────────────────────
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ───────────────────── Category ──────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ───────────────────── Listing ───────────────────────────
        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Description).HasColumnType("text").IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Location).HasMaxLength(100).IsRequired();

            // Enum → string
            entity.Property(e => e.Condition)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ListingStatus.Available);

            // Indexes (§7.2)
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => new { e.EducationLevelId, e.DepartmentId, e.SemesterOrClass });
            entity.HasIndex(e => e.Price);
            entity.HasIndex(e => e.Location);
            entity.HasIndex(e => e.SellerId);

            // Relationships
            entity.HasOne(e => e.Seller)
                .WithMany(u => u.Listings)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Listings)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EducationLevel)
                .WithMany(el => el.Listings)
                .HasForeignKey(e => e.EducationLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Listings)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ───────────────────── ListingImage ──────────────────────
        modelBuilder.Entity<ListingImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImagePath).HasMaxLength(255).IsRequired();

            entity.HasOne(e => e.Listing)
                .WithMany(l => l.Images)
                .HasForeignKey(e => e.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ───────────────────── Conversation ──────────────────────
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Unique: one conversation per (listing, buyer)
            entity.HasIndex(e => new { e.ListingId, e.BuyerId }).IsUnique();

            entity.HasOne(e => e.Listing)
                .WithMany(l => l.Conversations)
                .HasForeignKey(e => e.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Buyer)
                .WithMany(u => u.BuyerConversations)
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Seller)
                .WithMany(u => u.SellerConversations)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ───────────────────── Message ───────────────────────────
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Body).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            // Index for fetching messages in order (§7.2)
            entity.HasIndex(e => new { e.ConversationId, e.SentAt });

            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ───────────────────── Report ────────────────────────────
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.AdminNote).HasMaxLength(500);

            // Enum → string
            entity.Property(e => e.Reason)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ReportStatus.Pending);

            // Unique: one report per (listing, reporter)
            entity.HasIndex(e => new { e.ListingId, e.ReporterId }).IsUnique();

            entity.HasOne(e => e.Listing)
                .WithMany(l => l.Reports)
                .HasForeignKey(e => e.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reporter)
                .WithMany(u => u.Reports)
                .HasForeignKey(e => e.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
