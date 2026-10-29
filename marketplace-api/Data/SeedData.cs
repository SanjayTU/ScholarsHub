using Microsoft.EntityFrameworkCore;
using MarketplaceApi.Models;
using MarketplaceApi.Models.Enums;

namespace MarketplaceApi.Data;

/// <summary>
/// Seeds initial reference data: education levels, departments, categories, and one admin user.
/// Called from Program.cs at startup.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Seeds the database with initial data if tables are empty.
    /// The admin password is read from IConfiguration ("Admin:DefaultPassword").
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureCreatedAsync();

        await SeedEducationLevelsAsync(context);
        await SeedDepartmentsAsync(context);
        await SeedCategoriesAsync(context);
        await SeedAdminUserAsync(context, configuration);
    }

    // ───────────────── Education Levels ─────────────────
    private static async Task SeedEducationLevelsAsync(AppDbContext context)
    {
        if (await context.EducationLevels.AnyAsync()) return;

        var levels = new List<EducationLevel>
        {
            new() { Name = "Primary", Type = EducationLevelType.School, IsActive = true },
            new() { Name = "High School", Type = EducationLevelType.School, IsActive = true },
            new() { Name = "Higher Secondary", Type = EducationLevelType.School, IsActive = true },
            new() { Name = "Undergraduate", Type = EducationLevelType.College, IsActive = true },
            new() { Name = "Postgraduate", Type = EducationLevelType.College, IsActive = true }
        };

        context.EducationLevels.AddRange(levels);
        await context.SaveChangesAsync();
    }

    // ───────────────── Departments ───────────────────────
    private static async Task SeedDepartmentsAsync(AppDbContext context)
    {
        if (await context.Departments.AnyAsync()) return;

        var departments = new List<Department>
        {
            new() { Name = "Computer Applications", IsActive = true },
            new() { Name = "Computer Science", IsActive = true },
            new() { Name = "Commerce", IsActive = true },
            new() { Name = "Mathematics", IsActive = true },
            new() { Name = "Physics", IsActive = true },
            new() { Name = "Chemistry", IsActive = true },
            new() { Name = "English", IsActive = true },
            new() { Name = "Economics", IsActive = true },
            new() { Name = "Other", IsActive = true }
        };

        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();
    }

    // ───────────────── Categories (two-level tree) ──────
    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        // Parent categories (groups)
        var educationalMaterials = new Category { Name = "Educational Materials" };
        var stationery = new Category { Name = "Stationery" };
        var educationalEquipment = new Category { Name = "Educational Equipment" };
        var studentItems = new Category { Name = "Student Items" };

        context.Categories.AddRange(educationalMaterials, stationery, educationalEquipment, studentItems);
        await context.SaveChangesAsync();

        // Sub-categories
        var subCategories = new List<Category>
        {
            // Educational Materials
            new() { Name = "Textbooks", ParentId = educationalMaterials.Id },
            new() { Name = "Reference Books", ParentId = educationalMaterials.Id },
            new() { Name = "Notes", ParentId = educationalMaterials.Id },
            new() { Name = "Question Papers", ParentId = educationalMaterials.Id },
            new() { Name = "Study Materials", ParentId = educationalMaterials.Id },

            // Stationery
            new() { Name = "Notebooks", ParentId = stationery.Id },
            new() { Name = "Files", ParentId = stationery.Id },
            new() { Name = "Drawing Materials", ParentId = stationery.Id },
            new() { Name = "Pens/Pencils", ParentId = stationery.Id },
            new() { Name = "Other Stationery", ParentId = stationery.Id },

            // Educational Equipment
            new() { Name = "Scientific Calculators", ParentId = educationalEquipment.Id },
            new() { Name = "Geometry Instruments", ParentId = educationalEquipment.Id },
            new() { Name = "Lab Equipment", ParentId = educationalEquipment.Id },
            new() { Name = "Electronics Equipment", ParentId = educationalEquipment.Id },

            // Student Items
            new() { Name = "Bags", ParentId = studentItems.Id },
            new() { Name = "Study Tables", ParentId = studentItems.Id },
            new() { Name = "Lamps", ParentId = studentItems.Id },
            new() { Name = "Other Student Items", ParentId = studentItems.Id }
        };

        context.Categories.AddRange(subCategories);
        await context.SaveChangesAsync();
    }

    // ───────────────── Admin User ────────────────────────
    private static async Task SeedAdminUserAsync(AppDbContext context, IConfiguration configuration)
    {
        if (await context.Users.AnyAsync(u => u.Role == Role.Admin)) return;

        // Read admin password from configuration — never hard-code it
        var adminPassword = configuration["Admin:DefaultPassword"]
            ?? throw new InvalidOperationException(
                "Admin:DefaultPassword is not set in configuration. " +
                "Set it in appsettings.json, user-secrets, or an environment variable.");

        var firstLevel = await context.EducationLevels.FirstAsync();

        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@studentmarket.local",
            Mobile = "9000000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            EducationLevelId = firstLevel.Id,
            DepartmentId = null,
            Institution = "Admin",
            SemesterOrClass = 1,
            Locality = "Admin",
            Role = Role.Admin,
            Status = UserStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
