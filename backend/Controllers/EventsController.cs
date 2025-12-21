using Microsoft.AspNetCore.Mvc;
using TriviaApp.API.DTOs;
using TriviaApp.API.Services;

namespace TriviaApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EventDto>>> GetAllEvents()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var eventDto = await _eventService.GetEventByIdAsync(id);
        if (eventDto == null)
            return NotFound();

        return Ok(eventDto);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<EventDetailsDto>> GetEventDetails(int id)
    {
        var eventDto = await _eventService.GetEventDetailsAsync(id);
        if (eventDto == null)
            return NotFound();

        return Ok(eventDto);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent(CreateEventDto createEventDto)
    {
        var eventDto = await _eventService.CreateEventAsync(createEventDto);
        return CreatedAtAction(nameof(GetEvent), new { id = eventDto.Id }, eventDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EventDto>> UpdateEvent(int id, UpdateEventDto updateEventDto)
    {
        var eventDto = await _eventService.UpdateEventAsync(id, updateEventDto);
        if (eventDto == null)
            return NotFound();

        return Ok(eventDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var deleted = await _eventService.DeleteEventAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("rounds")]
    public async Task<ActionResult<RoundDto>> CreateRound(CreateRoundDto createRoundDto)
    {
        var roundDto = await _eventService.CreateRoundAsync(createRoundDto);
        return Ok(roundDto);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var categoryDto = await _eventService.CreateCategoryAsync(createCategoryDto);
        return Ok(categoryDto);
    }

    [HttpPost("questions")]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionDto createQuestionDto)
    {
        var questionDto = await _eventService.CreateQuestionAsync(createQuestionDto);
        return Ok(questionDto);
    }
}

