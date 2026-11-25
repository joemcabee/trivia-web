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

    public enum PresentationMode
    {
        QuestionsOnly,
        QuestionAnswer
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

        AddOpeningSlides(eventEntity, slides);

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

    [HttpGet("event/{eventId}/opening")]
    public async Task<ActionResult<PresentationDataDto>> GetOpeningSlides(int eventId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var eventEntity = await _context.Events
            .Where(e => e.Id == eventId && e.UserId == userId)
            .FirstOrDefaultAsync();

        if (eventEntity == null)
            return NotFound();

        var slides = new List<PresentationSlideDto>();
        AddOpeningSlides(eventEntity, slides);

        return Ok(new PresentationDataDto
        {
            EventId = eventEntity.Id,
            EventName = eventEntity.Name,
            Slides = slides
        });
    }

    private static void AddOpeningSlides(TriviaApp.API.Models.Event eventEntity, List<PresentationSlideDto> slides)
    {
        if (!string.IsNullOrWhiteSpace(eventEntity.WelcomeMessage))
        {
            slides.Add(new PresentationSlideDto
            {
                Type = "welcome",
                QuestionText = eventEntity.WelcomeMessage,
                EventDate = eventEntity.EventDate
            });
        }

        if (!string.IsNullOrWhiteSpace(eventEntity.RulesText))
        {
            slides.Add(new PresentationSlideDto
            {
                Type = "rules",
                QuestionText = "Rules",
                Rules = eventEntity.RulesText
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
            });
        }
    }

    [HttpGet("event/{eventId}/round/{roundId}")]
    public async Task<ActionResult<PresentationDataDto>> GetRoundPresentationData(
        int eventId,
        int roundId,
        [FromQuery] PresentationMode mode = PresentationMode.QuestionsOnly)
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

        var round = eventEntity.Rounds.FirstOrDefault(r => r.Id == roundId);
        if (round == null)
            return NotFound();

        var slides = new List<PresentationSlideDto>();

        foreach (var category in round.Categories.OrderBy(c => c.Order))
        {
            var questionNumber = 1;
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

                if (mode == PresentationMode.QuestionAnswer)
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
                }

                questionNumber++;
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
    public DateOnly? EventDate { get; set; }
    public List<string> Rules { get; set; } = new();
}
