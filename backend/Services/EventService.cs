using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
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

    private static string NormalizeTeamName(string name) => name.Trim();
    private static string NormalizeName(string name) => (name ?? string.Empty).Trim();
    private static string NormalizeRoundName(string value)
    {
        var trimmed = NormalizeName(value);
        if (string.IsNullOrWhiteSpace(trimmed))
            return trimmed;

        if (trimmed.StartsWith("Round ", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Round", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        if (int.TryParse(trimmed, out var roundNumber) && roundNumber > 0)
            return $"Round {roundNumber}";

        return trimmed;
    }

    private sealed record ImportRow(int RowNumber, string Round, string Category, string Question, string Answer);

    public async Task<List<EventDto>> GetAllEventsAsync(string userId)
    {
        var events = await _context.Events
            .Where(e => e.UserId == userId)
            .Include(e => e.Rounds)
                .ThenInclude(r => r.Categories)
                    .ThenInclude(c => c.Questions)
            .Include(e => e.Teams)
            .ToListAsync();

        return events.Select(e => new EventDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            CreatedAt = e.CreatedOn,
            UpdatedAt = e.UpdatedOn,
            RoundCount = e.Rounds.Count,
            TeamCount = e.Teams.Count,
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
            .Include(e => e.Teams)
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
            RoundCount = eventEntity.Rounds.Count,
            TeamCount = eventEntity.Teams.Count,
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
            RoundCount = 1,
            TeamCount = 0,
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

    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto createTeamDto, string userId)
    {
        var normalizedTeamName = NormalizeTeamName(createTeamDto.Name);
        if (string.IsNullOrWhiteSpace(normalizedTeamName))
            throw new InvalidOperationException("Team name is required.");

        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == createTeamDto.EventId && e.UserId == userId);
        if (!eventExists)
            throw new ArgumentException("Event not found or access denied.");

        var teamNameExists = await _context.Teams
            .AnyAsync(t => t.EventId == createTeamDto.EventId &&
                           t.Name.ToLower() == normalizedTeamName.ToLower());
        if (teamNameExists)
            throw new InvalidOperationException("Team name must be unique within an event.");

        var team = new Team
        {
            Name = normalizedTeamName,
            EventId = createTeamDto.EventId,
            CreatedOn = DateTime.UtcNow
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            EventId = team.EventId
        };
    }

    public async Task<TeamDto?> UpdateTeamAsync(int id, UpdateTeamDto updateTeamDto, string userId)
    {
        var normalizedTeamName = NormalizeTeamName(updateTeamDto.Name);
        if (string.IsNullOrWhiteSpace(normalizedTeamName))
            throw new InvalidOperationException("Team name is required.");

        var team = await _context.Teams
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (team == null || team.Event.UserId != userId)
            return null;

        var duplicateTeamName = await _context.Teams
            .AnyAsync(t => t.EventId == team.EventId &&
                           t.Id != team.Id &&
                           t.Name.ToLower() == normalizedTeamName.ToLower());
        if (duplicateTeamName)
            throw new InvalidOperationException("Team name must be unique within an event.");

        team.Name = normalizedTeamName;
        await _context.SaveChangesAsync();

        return new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            EventId = team.EventId
        };
    }

    public async Task<bool> DeleteTeamAsync(int id, string userId)
    {
        var team = await _context.Teams
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (team == null || team.Event.UserId != userId)
            return false;

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TeamPointDto>> SetRoundTeamPointsAsync(int roundId, SetRoundTeamPointsDto setRoundTeamPointsDto, string userId)
    {
        var roundTeamPoints = setRoundTeamPointsDto.TeamPoints ?? new List<SetRoundTeamPointItemDto>();

        if (roundTeamPoints.Count == 0)
            return new List<TeamPointDto>();

        var round = await _context.Rounds
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.Id == roundId);
        if (round == null || round.Event.UserId != userId)
            throw new ArgumentException("Round not found or access denied.");

        var teamIds = roundTeamPoints
            .Select(tp => tp.TeamId)
            .ToList();

        if (teamIds.Count != teamIds.Distinct().Count())
            throw new InvalidOperationException("Duplicate team IDs are not allowed in a single round update.");

        var matchingTeamIds = await _context.Teams
            .Where(t => t.EventId == round.EventId && teamIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        if (matchingTeamIds.Count != teamIds.Count)
            throw new InvalidOperationException("All teams must belong to the same event as the selected round.");

        var existingTeamPoints = await _context.TeamPoints
            .Where(tp => tp.RoundId == roundId && teamIds.Contains(tp.TeamId))
            .ToListAsync();

        var existingByTeamId = existingTeamPoints.ToDictionary(tp => tp.TeamId);

        foreach (var teamPointInput in roundTeamPoints)
        {
            if (existingByTeamId.TryGetValue(teamPointInput.TeamId, out var existingTeamPoint))
            {
                existingTeamPoint.Points = teamPointInput.Points;
            }
            else
            {
                _context.TeamPoints.Add(new TeamPoint
                {
                    TeamId = teamPointInput.TeamId,
                    RoundId = roundId,
                    Points = teamPointInput.Points,
                    CreatedOn = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        return await _context.TeamPoints
            .Where(tp => tp.RoundId == roundId && teamIds.Contains(tp.TeamId))
            .OrderBy(tp => tp.TeamId)
            .Select(tp => new TeamPointDto
            {
                Id = tp.Id,
                TeamId = tp.TeamId,
                RoundId = tp.RoundId,
                Points = tp.Points
            })
            .ToListAsync();
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
            RoundCount = eventEntity.Rounds.Count,
            TeamCount = eventEntity.Teams.Count,
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
            .Include(e => e.Teams)
                .ThenInclude(t => t.TeamPoints)
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
            }).ToList(),
            Teams = eventEntity.Teams
                .OrderBy(t => t.Name)
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    EventId = t.EventId
                })
                .ToList(),
            TeamPoints = eventEntity.Teams
                .SelectMany(t => t.TeamPoints)
                .OrderBy(tp => tp.RoundId)
                .ThenBy(tp => tp.TeamId)
                .Select(tp => new TeamPointDto
                {
                    Id = tp.Id,
                    TeamId = tp.TeamId,
                    RoundId = tp.RoundId,
                    Points = tp.Points
                })
                .ToList()
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
            RoundCount = 0,
            TeamCount = 0,
            CategoryCount = 0,
            QuestionCount = 0
        };
    }

    public async Task<ImportQuestionsResultDto> ImportQuestionsAsync(
        int eventId,
        Stream fileStream,
        string fileName,
        bool firstRowHasHeaders,
        string userId)
    {
        var eventEntity = await _context.Events
            .Where(e => e.Id == eventId && e.UserId == userId)
            .Include(e => e.Rounds)
                .ThenInclude(r => r.Categories)
            .FirstOrDefaultAsync();

        if (eventEntity == null)
            throw new ArgumentException("Event not found or access denied.");

        var rows = await ReadImportRowsAsync(fileStream, fileName, firstRowHasHeaders);

        var result = new ImportQuestionsResultDto
        {
            TotalRows = rows.Count
        };

        var roundComparer = StringComparer.OrdinalIgnoreCase;
        var roundByName = eventEntity.Rounds.ToDictionary(r => NormalizeName(r.Name), r => r, roundComparer);

        var maxRoundOrder = eventEntity.Rounds.Select(r => r.Order).DefaultIfEmpty(0).Max();

        // Existing category lookup keyed by (roundId, categoryName)
        var categoryByRoundAndName = new Dictionary<(int RoundId, string CategoryName), Category>();
        foreach (var round in eventEntity.Rounds)
        {
            foreach (var category in round.Categories)
            {
                categoryByRoundAndName[(round.Id, NormalizeName(category.Name))] = category;
            }
        }

        var existingCategoryIds = eventEntity.Rounds.SelectMany(r => r.Categories).Select(c => c.Id).ToList();
        var maxQuestionOrderByCategoryId = existingCategoryIds.Count == 0
            ? new Dictionary<int, int>()
            : await _context.Questions
                .Where(q => existingCategoryIds.Contains(q.CategoryId))
                .GroupBy(q => q.CategoryId)
                .Select(g => new { CategoryId = g.Key, MaxOrder = g.Max(q => q.Order) })
                .ToDictionaryAsync(x => x.CategoryId, x => x.MaxOrder);

        // Track orders for new categories that don't have IDs yet.
        var maxQuestionOrderByCategoryEntity = new Dictionary<Category, int>();

        foreach (var row in rows)
        {
            var roundName = NormalizeRoundName(row.Round);
            var categoryName = NormalizeName(row.Category);
            var questionText = NormalizeName(row.Question);
            var answerText = NormalizeName(row.Answer);

            if (string.IsNullOrWhiteSpace(roundName) ||
                string.IsNullOrWhiteSpace(categoryName) ||
                string.IsNullOrWhiteSpace(questionText) ||
                string.IsNullOrWhiteSpace(answerText))
            {
                result.SkippedRows++;
                result.Errors.Add(new ImportRowErrorDto
                {
                    RowNumber = row.RowNumber,
                    Message = "Round, Category, Question, and Answer are all required."
                });
                continue;
            }

            if (!roundByName.TryGetValue(roundName, out var roundEntity))
            {
                maxRoundOrder += 1;
                roundEntity = new Round
                {
                    Name = roundName,
                    EventId = eventEntity.Id,
                    Order = maxRoundOrder,
                    CreatedOn = DateTime.UtcNow
                };

                _context.Rounds.Add(roundEntity);
                eventEntity.Rounds.Add(roundEntity);
                roundByName[roundName] = roundEntity;
                result.CreatedRounds++;
            }

            Category categoryEntity;
            if (roundEntity.Id != 0)
            {
                if (!categoryByRoundAndName.TryGetValue((roundEntity.Id, categoryName), out categoryEntity!))
                {
                    var maxCategoryOrder = roundEntity.Categories.Select(c => c.Order).DefaultIfEmpty(0).Max();
                    categoryEntity = new Category
                    {
                        Name = categoryName,
                        RoundId = roundEntity.Id,
                        Order = maxCategoryOrder + 1,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.Categories.Add(categoryEntity);
                    roundEntity.Categories.Add(categoryEntity);
                    categoryByRoundAndName[(roundEntity.Id, categoryName)] = categoryEntity;
                    result.CreatedCategories++;
                }
            }
            else
            {
                // Round not saved yet; match on name within the in-memory collection.
                categoryEntity = roundEntity.Categories.FirstOrDefault(c => roundComparer.Equals(NormalizeName(c.Name), categoryName))
                    ?? CreateNewCategoryForUnsavedRound(roundEntity, categoryName, result);
            }

            var nextQuestionOrder = GetNextQuestionOrder(categoryEntity, maxQuestionOrderByCategoryId, maxQuestionOrderByCategoryEntity);

            var questionEntity = new Question
            {
                QuestionText = questionText,
                Answer = answerText,
                Category = categoryEntity,
                Order = nextQuestionOrder,
                CreatedOn = DateTime.UtcNow
            };

            _context.Questions.Add(questionEntity);
            categoryEntity.Questions.Add(questionEntity);
            result.ImportedQuestions++;
        }

        await _context.SaveChangesAsync();
        return result;
    }

    private static int GetNextQuestionOrder(
        Category category,
        Dictionary<int, int> maxOrderByCategoryId,
        Dictionary<Category, int> maxOrderByCategoryEntity)
    {
        if (category.Id != 0)
        {
            var current = maxOrderByCategoryId.TryGetValue(category.Id, out var max) ? max : 0;
            current += 1;
            maxOrderByCategoryId[category.Id] = current;
            return current;
        }

        var currentEntity = maxOrderByCategoryEntity.TryGetValue(category, out var maxEntity) ? maxEntity : 0;
        currentEntity += 1;
        maxOrderByCategoryEntity[category] = currentEntity;
        return currentEntity;
    }

    private static Category CreateNewCategoryForUnsavedRound(Round roundEntity, string categoryName, ImportQuestionsResultDto result)
    {
        var maxCategoryOrder = roundEntity.Categories.Select(c => c.Order).DefaultIfEmpty(0).Max();
        var category = new Category
        {
            Name = categoryName,
            Order = maxCategoryOrder + 1,
            CreatedOn = DateTime.UtcNow,
            Round = roundEntity
        };
        roundEntity.Categories.Add(category);
        result.CreatedCategories++;
        return category;
    }

    private static async Task<List<ImportRow>> ReadImportRowsAsync(Stream fileStream, string fileName, bool firstRowHasHeaders)
    {
        var ext = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        if (ext == ".csv")
            return await ReadCsvRowsAsync(fileStream, firstRowHasHeaders);

        if (ext == ".xlsx")
            return ReadXlsxRows(fileStream, firstRowHasHeaders);

        throw new InvalidOperationException("Unsupported file type. Please upload a .csv or .xlsx file.");
    }

    private static async Task<List<ImportRow>> ReadCsvRowsAsync(Stream fileStream, bool firstRowHasHeaders)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = firstRowHasHeaders,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true,
            BadDataFound = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(fileStream, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        var rows = new List<ImportRow>();
        var rowNumber = 0;

        if (firstRowHasHeaders)
        {
            if (!await csv.ReadAsync())
                return rows;
            csv.ReadHeader();

            var headers = csv.HeaderRecord ?? Array.Empty<string>();
            var required = new[] { "Round", "Category", "Question", "Answer" };
            foreach (var req in required)
            {
                if (!headers.Any(h => string.Equals(h?.Trim(), req, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"CSV header row must include a '{req}' column.");
            }
        }

        while (await csv.ReadAsync())
        {
            rowNumber++;

            string round;
            string category;
            string question;
            string answer;

            if (firstRowHasHeaders)
            {
                round = GetCsvFieldCaseInsensitive(csv, "Round");
                category = GetCsvFieldCaseInsensitive(csv, "Category");
                question = GetCsvFieldCaseInsensitive(csv, "Question");
                answer = GetCsvFieldCaseInsensitive(csv, "Answer");
            }
            else
            {
                round = csv.GetField(0) ?? string.Empty;
                category = csv.GetField(1) ?? string.Empty;
                question = csv.GetField(2) ?? string.Empty;
                answer = csv.GetField(3) ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(round) &&
                string.IsNullOrWhiteSpace(category) &&
                string.IsNullOrWhiteSpace(question) &&
                string.IsNullOrWhiteSpace(answer))
            {
                continue;
            }

            rows.Add(new ImportRow(
                RowNumber: firstRowHasHeaders ? rowNumber + 1 : rowNumber,
                Round: round,
                Category: category,
                Question: question,
                Answer: answer));
        }

        return rows;
    }

    private static string GetCsvFieldCaseInsensitive(CsvReader csv, string name)
    {
        var headers = csv.HeaderRecord ?? Array.Empty<string>();
        for (var i = 0; i < headers.Length; i++)
        {
            if (string.Equals(headers[i]?.Trim(), name, StringComparison.OrdinalIgnoreCase))
                return csv.GetField(i) ?? string.Empty;
        }
        return string.Empty;
    }

    private static List<ImportRow> ReadXlsxRows(Stream fileStream, bool firstRowHasHeaders)
    {
        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheets.FirstOrDefault();
        if (ws == null)
            return new List<ImportRow>();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
        if (lastRow == 0)
            return new List<ImportRow>();

        int colRound = 1, colCategory = 2, colQuestion = 3, colAnswer = 4;
        var startRow = 1;

        if (firstRowHasHeaders)
        {
            startRow = 2;
            var headerRow = ws.Row(1);

            colRound = FindHeaderColumn(headerRow, "Round") ?? throw new InvalidOperationException("Spreadsheet header row must include a 'Round' column.");
            colCategory = FindHeaderColumn(headerRow, "Category") ?? throw new InvalidOperationException("Spreadsheet header row must include a 'Category' column.");
            colQuestion = FindHeaderColumn(headerRow, "Question") ?? throw new InvalidOperationException("Spreadsheet header row must include a 'Question' column.");
            colAnswer = FindHeaderColumn(headerRow, "Answer") ?? throw new InvalidOperationException("Spreadsheet header row must include an 'Answer' column.");
        }

        var rows = new List<ImportRow>();
        for (var r = startRow; r <= lastRow; r++)
        {
            var round = ws.Cell(r, colRound).GetString();
            var category = ws.Cell(r, colCategory).GetString();
            var question = ws.Cell(r, colQuestion).GetString();
            var answer = ws.Cell(r, colAnswer).GetString();

            if (string.IsNullOrWhiteSpace(round) &&
                string.IsNullOrWhiteSpace(category) &&
                string.IsNullOrWhiteSpace(question) &&
                string.IsNullOrWhiteSpace(answer))
            {
                continue;
            }

            rows.Add(new ImportRow(
                RowNumber: r,
                Round: round,
                Category: category,
                Question: question,
                Answer: answer));
        }

        return rows;
    }

    private static int? FindHeaderColumn(IXLRow headerRow, string headerName)
    {
        var lastCell = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (var c = 1; c <= lastCell; c++)
        {
            var val = headerRow.Cell(c).GetString();
            if (string.Equals(val?.Trim(), headerName, StringComparison.OrdinalIgnoreCase))
                return c;
        }

        return null;
    }

    public async Task<List<QuestionSearchResultDto>> SearchQuestionsAsync(string userId, string query, int? excludeEventId, int limit)
    {
        query = (query ?? string.Empty).Trim();
        if (query.Length < 2)
            return new List<QuestionSearchResultDto>();

        // Guardrail against pathological queries.
        limit = Math.Clamp(limit, 1, 100);

        var tokens = query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(8)
            .ToArray();

        if (tokens.Length == 0)
            return new List<QuestionSearchResultDto>();

        IQueryable<Question> questions = _context.Questions
            .AsNoTracking()
            .Where(q => q.Category.Round.Event.UserId == userId);

        if (excludeEventId.HasValue)
            questions = questions.Where(q => q.Category.Round.EventId != excludeEventId.Value);

        foreach (var token in tokens)
        {
            var pattern = $"%{token}%";
            questions = questions.Where(q =>
                EF.Functions.ILike(q.QuestionText, pattern) ||
                EF.Functions.ILike(q.Answer, pattern));
        }

        return await questions
            .OrderByDescending(q => q.CreatedOn)
            .ThenByDescending(q => q.Id)
            .Select(q => new QuestionSearchResultDto
            {
                QuestionId = q.Id,
                EventId = q.Category.Round.EventId,
                EventName = q.Category.Round.Event.Name,
                CategoryId = q.CategoryId,
                CategoryName = q.Category.Name,
                QuestionText = q.QuestionText,
                Answer = q.Answer
            })
            .Take(limit)
            .ToListAsync();
    }
}
