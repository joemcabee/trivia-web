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
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto createQuestionDto);
    Task<EventDto?> GetEventWithDetailsAsync(int id);
    Task<EventDetailsDto?> GetEventDetailsAsync(int id);
}

