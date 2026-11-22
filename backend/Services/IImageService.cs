namespace TriviaApp.API.Services;

using TriviaApp.API.DTOs;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile file);
    Task<bool> DeleteImageAsync(string imageUrl);
    Task<IReadOnlyList<StockImageDto>> GetStockImagesAsync();
}

