namespace LuxRentals.Services.Cars;

public class CarImageStorage : ICarImageStorage
{
    private const long MAX_IMAGE_SIZE_BYTES = 5 * 1024 * 1024;
    private const string UPLOADS_ROOT = "uploads";
    private const string CARS_FOLDER = "cars";

    private static readonly HashSet<string> AllowedImageExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CarImageStorage> _logger;

    public CarImageStorage(IWebHostEnvironment environment, ILogger<CarImageStorage> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public IReadOnlyList<string> Validate(IFormFile? file)
    {
        var errors = new List<string>();

        if (file is null)
            return errors;

        if (file.Length == 0)
        {
            errors.Add("Select a non-empty image file.");
            return errors;
        }

        if (file.Length > MAX_IMAGE_SIZE_BYTES)
        {
            errors.Add("Image size must be 5 MB or less.");
            return errors;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            errors.Add("Only JPG, PNG, and WEBP images are allowed.");

        return errors;
    }

    public async Task<string?> SaveNewAsync(IFormFile file)
    {
        try
        {
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new InvalidOperationException("Web root path is not configured.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var relativePath = $"/{UPLOADS_ROOT}/{CARS_FOLDER}/{fileName}";
            var absoluteDirectory = Path.Combine(webRootPath, UPLOADS_ROOT, CARS_FOLDER);
            var absolutePath = Path.Combine(absoluteDirectory, fileName);

            Directory.CreateDirectory(absoluteDirectory);

            await using var stream = File.Create(absolutePath);
            await file.CopyToAsync(stream);

            return relativePath;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Failed to save uploaded car image.");
            return null;
        }
    }

    // Deletes only files that resolve inside the car uploads directory so an arbitrary relative path
    // cannot be used to remove other content under wwwroot.
    public Task DeleteAsync(string? relativePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.CompletedTask;

            var trimmedPath = relativePath.TrimStart('/', '\\');
            var normalizedPath = trimmedPath.Replace('/', Path.DirectorySeparatorChar);
            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                return Task.CompletedTask;

            var absolutePath = Path.GetFullPath(Path.Combine(webRootPath, normalizedPath));
            var uploadsRoot = Path.GetFullPath(Path.Combine(webRootPath, UPLOADS_ROOT, CARS_FOLDER));

            if (!absolutePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            if (File.Exists(absolutePath))
                File.Delete(absolutePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Failed to delete car image at path {RelativePath}.", relativePath);
        }

        return Task.CompletedTask;
    }
}
