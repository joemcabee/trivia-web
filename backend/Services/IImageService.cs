namespace TriviaApp.API.Services;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile file);
    Task<bool> DeleteImageAsync(string imageUrl);
}

