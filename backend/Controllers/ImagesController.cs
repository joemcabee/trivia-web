using Microsoft.AspNetCore.Mvc;
using TriviaApp.API.Services;

namespace TriviaApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        try
        {
            var imageUrl = await _imageService.SaveImageAsync(file);
            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteImage([FromQuery] string imageUrl)
    {
        var deleted = await _imageService.DeleteImageAsync(imageUrl);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

