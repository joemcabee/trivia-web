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

    public async Task<List<EventDto>> GetAllEventsAsync(string userId)
    {
        var events = await _context.Events
            .Where(e => e.UserId == userId)
            .Include(e => e.Rounds)
                .ThenInclude(r => r.Categories)
                    .ThenInclude(c => c.Questions)
            .ToListAsync();

        return events.Select(e => new EventDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            CreatedAt = e.CreatedOn,
            UpdatedAt = e.UpdatedOn,
            CategoryCount = e.Rounds.SelectMany(r => r.Categories).Count(),
            QuestionCount = e.Rounds.SelectMany(r => r.Categories).SelectMany(c => c.Questions).Count()
        }).ToList();
    }

    public async Task<EventDto?> GetEventByIdAsync(int id, string userId)
    {
        var eventEntity = await _context.Events
            .Where(e => e.UserId == userId)
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
            CreatedAt = eventEntity.CreatedOn,
            UpdatedAt = eventEntity.UpdatedOn,
            CategoryCount = eventEntity.Rounds.SelectMany(r => r.Categories).Count(),
            QuestionCount = eventEntity.Rounds.SelectMany(r => r.Categories).SelectMany(c => c.Questions).Count()
        };
    }

    public async Task<EventDto> CreateEventAsync(CreateEventDto createEventDto, string userId)
    {
        var eventEntity = new Event
        {
            Name = createEventDto.Name,
            Description = createEventDto.Description,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            UserId = userId
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var round = new Round
        {
            Name = "Round 1",
            EventId = eventEntity.Id,
            Order = 1,
            CreatedOn = DateTime.UtcNow
        };
        _context.Rounds.Add(round);
        await _context.SaveChangesAsync();

        return new EventDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            CreatedAt = eventEntity.CreatedOn,
            UpdatedAt = eventEntity.UpdatedOn,
            CategoryCount = 0,
            QuestionCount = 0
        };
    }

    public async Task<EventDto?> UpdateEventAsync(int id, UpdateEventDto updateEventDto, string userId)
    {
        var eventEntity = await _context.Events
            .Where(e => e.Id == id && e.UserId == userId)
            .FirstOrDefaultAsync();
        if (eventEntity == null)
            return null;

        eventEntity.Name = updateEventDto.Name;
        eventEntity.Description = updateEventDto.Description;
        eventEntity.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetEventByIdAsync(id, userId);
    }

    public async Task<bool> DeleteEventAsync(int id, string userId)
    {
        var eventEntity = await _context.Events
            .Where(e => e.Id == id && e.UserId == userId)
            .FirstOrDefaultAsync();
        if (eventEntity == null)
            return false;

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RoundDto> CreateRoundAsync(CreateRoundDto createRoundDto, string userId)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.Id == createRoundDto.EventId && e.UserId == userId);
        if (!eventExists)
            throw new ArgumentException("Event not found or access denied.");

        var maxOrder = await _context.Rounds
            .Where(r => r.EventId == createRoundDto.EventId)
            .OrderByDescending(r => r.Order)
            .Select(r => r.Order)
            .FirstOrDefaultAsync();

        var round = new Round
        {
            Name = createRoundDto.Name,
            EventId = createRoundDto.EventId,
            Order = maxOrder + 1,
            CreatedOn = DateTime.UtcNow
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

    public async Task<bool> DeleteRoundAsync(int id, string userId)
    {
        var round = await _context.Rounds
            .Include(r => r.Event)
            .Include(r => r.Categories)
                .ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (round == null || round.Event.UserId != userId)
            return false;

        // Delete all questions in categories of this round
        foreach (var category in round.Categories)
        {
            _context.Questions.RemoveRange(category.Questions);
        }

        // Delete all categories in this round
        _context.Categories.RemoveRange(round.Categories);

        // Delete the round
        _context.Rounds.Remove(round);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, string userId)
    {
        var round = await _context.Rounds.Include(r => r.Event).FirstOrDefaultAsync(r => r.Id == createCategoryDto.RoundId);
        if (round == null || round.Event.UserId != userId)
            throw new ArgumentException("Round not found or access denied.");

        var maxOrder = await _context.Categories
            .Where(c => c.RoundId == createCategoryDto.RoundId)
            .OrderByDescending(c => c.Order)
            .Select(c => c.Order)
            .FirstOrDefaultAsync();

        var category = new Category
        {
            Name = createCategoryDto.Name,
            RoundId = createCategoryDto.RoundId,
            Order = maxOrder + 1,
            CreatedOn = DateTime.UtcNow
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

    public async Task<bool> DeleteCategoryAsync(int id, string userId)
    {
        var category = await _context.Categories
            .Include(c => c.Round)
                .ThenInclude(r => r.Event)
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null || category.Round.Event.UserId != userId)
            return false;

        // Delete all questions in this category
        _context.Questions.RemoveRange(category.Questions);

        // Delete the category
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto createQuestionDto, string userId)
    {
        var category = await _context.Categories
            .Include(c => c.Round)
                .ThenInclude(r => r.Event)
            .FirstOrDefaultAsync(c => c.Id == createQuestionDto.CategoryId);
        if (category == null || category.Round.Event.UserId != userId)
            throw new ArgumentException("Category not found or access denied.");

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
            Order = maxOrder + 1,
            CreatedOn = DateTime.UtcNow
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

    public async Task<QuestionDto?> UpdateQuestionAsync(int id, UpdateQuestionDto updateQuestionDto, string userId)
    {
        var question = await _context.Questions
            .Include(q => q.Category)
                .ThenInclude(c => c.Round)
                    .ThenInclude(r => r.Event)
            .FirstOrDefaultAsync(q => q.Id == id);
        if (question == null || question.Category.Round.Event.UserId != userId)
            return null;

        question.QuestionText = updateQuestionDto.QuestionText;
        question.Answer = updateQuestionDto.Answer;
        question.ImageUrl = updateQuestionDto.ImageUrl;

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

    public async Task<bool> DeleteQuestionAsync(int id, string userId)
    {
        var question = await _context.Questions
            .Include(q => q.Category)
                .ThenInclude(c => c.Round)
                    .ThenInclude(r => r.Event)
            .FirstOrDefaultAsync(q => q.Id == id);
        if (question == null || question.Category.Round.Event.UserId != userId)
            return false;

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EventDto?> GetEventWithDetailsAsync(int id, string userId)
    {
        var eventEntity = await _context.Events
            .Where(e => e.UserId == userId)
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
            CreatedAt = eventEntity.CreatedOn,
            UpdatedAt = eventEntity.UpdatedOn,
            CategoryCount = eventEntity.Rounds.SelectMany(r => r.Categories).Count(),
            QuestionCount = eventEntity.Rounds.SelectMany(r => r.Categories).SelectMany(c => c.Questions).Count()
        };
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(int id, string userId)
    {
        var eventEntity = await _context.Events
            .Where(e => e.UserId == userId)
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
            CreatedAt = eventEntity.CreatedOn,
            UpdatedAt = eventEntity.UpdatedOn,
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

    public async Task<EventDto> CloneEventAsync(int id, string userId)
    {
        var originalEvent = await _context.Events
            .Where(e => e.Id == id && e.UserId == userId)
            .Include(e => e.Rounds)
                .ThenInclude(r => r.Categories)
                    .ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync();

        if (originalEvent == null)
            throw new ArgumentException($"Event with id {id} not found");

        var clonedEvent = new Event
        {
            Name = $"Copy of {originalEvent.Name}",
            Description = originalEvent.Description,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            UserId = userId
        };

        _context.Events.Add(clonedEvent);
        await _context.SaveChangesAsync();

        // Clone rounds
        foreach (var originalRound in originalEvent.Rounds.OrderBy(r => r.Order))
        {
            var clonedRound = new Round
            {
                Name = originalRound.Name,
                EventId = clonedEvent.Id,
                Order = originalRound.Order,
                CreatedOn = DateTime.UtcNow
            };

            _context.Rounds.Add(clonedRound);
            await _context.SaveChangesAsync();

            // Clone categories
            foreach (var originalCategory in originalRound.Categories.OrderBy(c => c.Order))
            {
                var clonedCategory = new Category
                {
                    Name = originalCategory.Name,
                    RoundId = clonedRound.Id,
                    Order = originalCategory.Order,
                    CreatedOn = DateTime.UtcNow
                };

                _context.Categories.Add(clonedCategory);
                await _context.SaveChangesAsync();

                // Clone questions
                foreach (var originalQuestion in originalCategory.Questions.OrderBy(q => q.Order))
                {
                    var clonedQuestion = new Question
                    {
                        QuestionText = originalQuestion.QuestionText,
                        Answer = originalQuestion.Answer,
                        ImageUrl = originalQuestion.ImageUrl,
                        CategoryId = clonedCategory.Id,
                        Order = originalQuestion.Order,
                        CreatedOn = DateTime.UtcNow
                    };

                    _context.Questions.Add(clonedQuestion);
                }
            }
        }

        await _context.SaveChangesAsync();

        // Detach the cloned event to ensure fresh query
        _context.Entry(clonedEvent).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        // Reload the event with all relationships to ensure it's fully available
        var clonedEventDto = await GetEventByIdAsync(clonedEvent.Id, userId);

        return clonedEventDto ?? new EventDto
        {
            Id = clonedEvent.Id,
            Name = clonedEvent.Name,
            Description = clonedEvent.Description,
            CreatedAt = clonedEvent.CreatedOn,
            UpdatedAt = clonedEvent.UpdatedOn,
            CategoryCount = 0,
            QuestionCount = 0
        };
    }
}

