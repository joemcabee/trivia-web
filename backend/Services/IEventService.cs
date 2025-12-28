using TriviaApp.API.DTOs;

namespace TriviaApp.API.Services;

public interface IEventService
{
    Task<List<EventDto>> GetAllEventsAsync();
    Task<EventDto?> GetEventByIdAsync(int id);
    Task<EventDto> CreateEventAsync(CreateEventDto createEventDto);
    Task<EventDto?> UpdateEventAsync(int id, UpdateEventDto updateEventDto);
    Task<bool> DeleteEventAsync(int id);
    Task<RoundDto> CreateRoundAsync(CreateRoundDto createRoundDto);
    Task<bool> DeleteRoundAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    Task<bool> DeleteCategoryAsync(int id);
    Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto createQuestionDto);
    Task<QuestionDto?> UpdateQuestionAsync(int id, UpdateQuestionDto updateQuestionDto);
    Task<bool> DeleteQuestionAsync(int id);
    Task<EventDto?> GetEventWithDetailsAsync(int id);
    Task<EventDetailsDto?> GetEventDetailsAsync(int id);
    Task<EventDto> CloneEventAsync(int id);
}

