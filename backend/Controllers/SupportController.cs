using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriviaApp.API.Data;
using TriviaApp.API.Models;

namespace TriviaApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly ApplicationDbContext _applicationDbContext;

    public SupportController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupportRequest([FromBody] CreateSupportRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.MessageText))
            return BadRequest("Message text is required.");

        var support = new Support
        {
            UserId = userId,
            Application = "trivia-app",
            MessageText = request.MessageText.Trim(),
            CreatedOn = DateTime.UtcNow,
            IsOpen = true
        };

        _applicationDbContext.Supports.Add(support);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new { supportId = support.SupportId });
    }
}

public class CreateSupportRequest
{
    public string MessageText { get; set; } = string.Empty;
}
