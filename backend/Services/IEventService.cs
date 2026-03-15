using TriviaApp.API.DTOs;

namespace TriviaApp.API.Services;

public interface IEventService
{
    Task<List<EventDto>> GetAllEventsAsync(string userId);
    Task<EventDto?> GetEventByIdAsync(int id, string userId);
    Task<EventDto> CreateEventAsync(CreateEventDto createEventDto, string userId);
    Task<EventDto?> UpdateEventAsync(int id, UpdateEventDto updateEventDto, string userId);
    Task<bool> DeleteEventAsync(int id, string userId);
    Task<RoundDto> CreateRoundAsync(CreateRoundDto createRoundDto, string userId);
    Task<bool> DeleteRoundAsync(int id, string userId);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, string userId);
    Task<bool> DeleteCategoryAsync(int id, string userId);
    Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto createQuestionDto, string userId);
    Task<QuestionDto?> UpdateQuestionAsync(int id, UpdateQuestionDto updateQuestionDto, string userId);
    Task<bool> DeleteQuestionAsync(int id, string userId);
    Task<EventDto?> GetEventWithDetailsAsync(int id, string userId);
    Task<EventDetailsDto?> GetEventDetailsAsync(int id, string userId);
    Task<EventDto> CloneEventAsync(int id, string userId);
}
