using Microsoft.EntityFrameworkCore;
using TriviaApp.API.Data;
using TriviaApp.API.DTOs;
using TriviaApp.API.Models;

namespace TriviaApp.API.Services;

public class EventService : IEventService
{
    private readonly TriviaDbContext _context;

    public EventService(TriviaDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventDto>> GetAllEventsAsync()
    {
        var events = await _context.Events
            .Include(e => e.Rounds)
                .ThenInclude(r => r.Categories)
                    .ThenInclude(c => c.Questions)
            .ToListAsync();

        return events.Select(e => new EventDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            CategoryCount = e.Rounds.SelectMany(r => r.Categories).Count(),
            QuestionCount = e.Rounds.SelectMany(r => r.Categories).SelectMany(c => c.Questions).Count()
        }).ToList();
    }

    public async Task<EventDto?> GetEventByIdAsync(int id)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Rounds)
                .ThenInclude(r => r.Categories)
                    .ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventEntity == null)
            return null;

        return new EventDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt,
            CategoryCount = eventEntity.Rounds.SelectMany(r => r.Categories).Count(),
            QuestionCount = eventEntity.Rounds.SelectMany(r => r.Categories).SelectMany(c => c.Questions).Count()
        };
    }

    public async Task<EventDto> CreateEventAsync(CreateEventDto createEventDto)
    {
        var eventEntity = new Event
        {
            Name = createEventDto.Name,
            Description = createEventDto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        // Create a default round
        var round = new Round
        {
            Name = "Round 1",
            EventId = eventEntity.Id,
            Order = 1
        };
        _context.Rounds.Add(round);
        await _context.SaveChangesAsync();

        return new EventDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt,
            CategoryCount = 0,
            QuestionCount = 0
        };
    }

    public async Task<EventDto?> UpdateEventAsync(int id, UpdateEventDto updateEventDto)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
            return null;

        eventEntity.Name = updateEventDto.Name;
        eventEntity.Description = updateEventDto.Description;
        eventEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetEventByIdAsync(id);
    }

    public async Task<bool> DeleteEventAsync(int id)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
            return false;

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RoundDto> CreateRoundAsync(CreateRoundDto createRoundDto)
    {
        var maxOrder = await _context.Rounds
            .Where(r => r.EventId == createRoundDto.EventId)
            .OrderByDescending(r => r.Order)
            .Select(r => r.Order)
            .FirstOrDefaultAsync();

        var round = new Round
        {
            Name = createRoundDto.Name,
            EventId = createRoundDto.EventId,
            Order = maxOrder + 1
        };

        _context.Rounds.Add(round);
        await _context.SaveChangesAsync();

        return new RoundDto
        {
            Id = round.Id,
            Name = round.Name,
            EventId = round.EventId,
            Order = round.Order,
            Categories = new List<CategoryDto>()
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        var maxOrder = await _context.Categories
            .Where(c => c.RoundId == createCategoryDto.RoundId)
            .OrderByDescending(c => c.Order)
            .Select(c => c.Order)
            .FirstOrDefaultAsync();

        var category = new Category
        {
            Name = createCategoryDto.Name,
            RoundId = createCategoryDto.RoundId,
            Order = maxOrder + 1
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            RoundId = category.RoundId,
            Order = category.Order,
            Questions = new List<QuestionDto>()
        };
    }

    public async Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto createQuestionDto)
    {
        var maxOrder = await _context.Questions
            .Where(q => q.CategoryId == createQuestionDto.CategoryId)
            .OrderByDescending(q => q.Order)
            .Select(q => q.Order)
            .FirstOrDefaultAsync();

        var question = new Question
        {
            QuestionText = createQuestionDto.QuestionText,
            Answer = createQuestionDto.Answer,
            ImageUrl = createQuestionDto.ImageUrl,
            CategoryId = createQuestionDto.CategoryId,
            Order = maxOrder + 1
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        return new QuestionDto
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            Answer = question.Answer,
            ImageUrl = question.ImageUrl,
            CategoryId = question.CategoryId,
            Order = question.Order
        };
    }

    public async Task<EventDto?> GetEventWithDetailsAsync(int id)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Rounds)
                .ThenInclude(r => r.Categories)
                    .ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventEntity == null)
            return null;

        return new EventDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt,
            CategoryCount = eventEntity.Rounds.SelectMany(r => r.Categories).Count(),
            QuestionCount = eventEntity.Rounds.SelectMany(r => r.Categories).SelectMany(c => c.Questions).Count()
        };
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(int id)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Rounds)
                .ThenInclude(r => r.Categories)
                    .ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventEntity == null)
            return null;

        return new EventDetailsDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt,
            Rounds = eventEntity.Rounds.OrderBy(r => r.Order).Select(r => new RoundDetailsDto
            {
                Id = r.Id,
                Name = r.Name,
                EventId = r.EventId,
                Order = r.Order,
                Categories = r.Categories.OrderBy(c => c.Order).Select(c => new CategoryDetailsDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    RoundId = c.RoundId,
                    Order = c.Order,
                    Questions = c.Questions.OrderBy(q => q.Order).Select(q => new QuestionDto
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        Answer = q.Answer,
                        ImageUrl = q.ImageUrl,
                        CategoryId = q.CategoryId,
                        Order = q.Order
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }
}

