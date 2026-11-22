namespace TriviaApp.API.Services;

using System.Text.Json;
using TriviaApp.API.DTOs;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _environment;
    private const string ImagesFolder = "images";

    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(wwwrootPath, ImagesFolder);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/{ImagesFolder}/{uniqueFileName}";
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl) ||
            !imageUrl.StartsWith($"/{ImagesFolder}/", StringComparison.OrdinalIgnoreCase) ||
            imageUrl.Contains("..", StringComparison.Ordinal))
            return false;

        try
        {
            var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            var filePath = imageUrl.StartsWith("/") 
                ? Path.Combine(wwwrootPath, imageUrl.TrimStart('/'))
                : Path.Combine(wwwrootPath, ImagesFolder, imageUrl);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return await Task.FromResult(true);
            }
        }
        catch
        {
            // Log error if needed
        }

        return false;
    }

    public async Task<IReadOnlyList<StockImageDto>> GetStockImagesAsync()
    {
        var manifestPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "shared", "stock-images.json");
        if (!File.Exists(manifestPath))
            return Array.Empty<StockImageDto>();

        await using var stream = File.OpenRead(manifestPath);
        var manifest = await JsonSerializer.DeserializeAsync<StockImageManifest>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return manifest?.Images ?? (IReadOnlyList<StockImageDto>)Array.Empty<StockImageDto>();
    }

    private sealed class StockImageManifest
    {
        public List<StockImageDto> Images { get; set; } = new();
    }
}

