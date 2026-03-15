using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriviaApp.API.Data;
using TriviaApp.API.DTOs;

namespace TriviaApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PresentationController : ControllerBase
{
    private readonly TriviaDbContext _context;

    public PresentationController(TriviaDbContext context)
    {
        _context = context;
    }

    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<PresentationDataDto>> GetPresentationData(int eventId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var eventEntity = await _context.Events
            .Where(e => e.Id == eventId && e.UserId == userId)
            .Include(e => e.Rounds)
                .ThenInclude(r => r.Categories)
                    .ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync();

        if (eventEntity == null)
            return NotFound();

        var slides = new List<PresentationSlideDto>();

        foreach (var round in eventEntity.Rounds.OrderBy(r => r.Order))
        {
            foreach (var category in round.Categories.OrderBy(c => c.Order))
            {
                var questionNumber = 1;
                // Add question slides (without answers)
                foreach (var question in category.Questions.OrderBy(q => q.Order))
                {
                    slides.Add(new PresentationSlideDto
                    {
                        Type = "question",
                        RoundName = round.Name,
                        CategoryName = category.Name,
                        QuestionText = question.QuestionText,
                        Answer = null,
                        ImageUrl = question.ImageUrl,
                        QuestionId = question.Id,
                        QuestionNumber = questionNumber
                    });
                    questionNumber++;
                }

                questionNumber = 1;
                // Add answer slides (with answers)
                foreach (var question in category.Questions.OrderBy(q => q.Order))
                {
                    slides.Add(new PresentationSlideDto
                    {
                        Type = "answer",
                        RoundName = round.Name,
                        CategoryName = category.Name,
                        QuestionText = question.QuestionText,
                        Answer = question.Answer,
                        ImageUrl = question.ImageUrl,
                        QuestionId = question.Id,
                        QuestionNumber = questionNumber
                    });
                    questionNumber++;
                }
            }
        }

        return Ok(new PresentationDataDto
        {
            EventId = eventEntity.Id,
            EventName = eventEntity.Name,
            Slides = slides
        });
    }
}

public class PresentationDataDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public List<PresentationSlideDto> Slides { get; set; } = new();
}

public class PresentationSlideDto
{
    public string Type { get; set; } = string.Empty; // "question" or "answer"
    public string RoundName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public string? ImageUrl { get; set; }
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
}

