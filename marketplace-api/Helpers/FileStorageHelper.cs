namespace MarketplaceApi.Helpers;

public class FileStorageHelper
{
    public const long MaxFileSizeBytes = 2 * 1024 * 1024;
    public const int MaxImagesPerListing = 5;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly IWebHostEnvironment _env;

    public FileStorageHelper(IWebHostEnvironment env) => _env = env;

    public string UploadsRoot => Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");

    public (bool Valid, Dictionary<string, string[]>? Errors) ValidateImages(IList<IFormFile> files, bool required)
    {
        var errors = new Dictionary<string, string[]>();

        if (files is null || files.Count == 0)
        {
            if (required)
                errors["Images"] = ["At least 1 image is required."];
            return (errors.Count == 0, errors.Count == 0 ? null : errors);
        }

        if (files.Count > MaxImagesPerListing)
            errors["Images"] = [$"A listing can have at most {MaxImagesPerListing} images."];

        var fileErrors = new List<string>();
        for (var i = 0; i < files.Count; i++)
        {
            var file = files[i];
            if (file.Length <= 0)
            {
                fileErrors.Add($"Image {i + 1} is empty.");
                continue;
            }

            if (file.Length > MaxFileSizeBytes)
                fileErrors.Add($"Image {i + 1} exceeds the 2 MB limit.");

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
                fileErrors.Add($"Image {i + 1} must be jpg, png, or webp.");
        }

        if (fileErrors.Count > 0)
            errors["Images"] = errors.TryGetValue("Images", out var existing)
                ? existing.Concat(fileErrors).ToArray()
                : fileErrors.ToArray();

        return (errors.Count == 0, errors.Count == 0 ? null : errors);
    }

    public async Task<List<string>> SaveListingImagesAsync(IList<IFormFile> files, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(UploadsRoot);
        var paths = new List<string>(files.Count);

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext == ".jpeg") ext = ".jpg";

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(UploadsRoot, fileName);

            await using var stream = new FileStream(fullPath, FileMode.CreateNew);
            await file.CopyToAsync(stream, cancellationToken);

            paths.Add($"uploads/{fileName}");
        }

        return paths;
    }

    public void DeleteFiles(IEnumerable<string> relativePaths)
    {
        foreach (var relative in relativePaths)
        {
            if (string.IsNullOrWhiteSpace(relative)) continue;
            var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
                relative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
