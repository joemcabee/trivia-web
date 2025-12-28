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

    [HttpDelete("rounds/{id}")]
    public async Task<IActionResult> DeleteRound(int id)
    {
        var deleted = await _eventService.DeleteRoundAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("categories")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var categoryDto = await _eventService.CreateCategoryAsync(createCategoryDto);
        return Ok(categoryDto);
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deleted = await _eventService.DeleteCategoryAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("questions")]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionDto createQuestionDto)
    {
        var questionDto = await _eventService.CreateQuestionAsync(createQuestionDto);
        return Ok(questionDto);
    }

    [HttpPut("questions/{id}")]
    public async Task<ActionResult<QuestionDto>> UpdateQuestion(int id, UpdateQuestionDto updateQuestionDto)
    {
        var questionDto = await _eventService.UpdateQuestionAsync(id, updateQuestionDto);
        if (questionDto == null)
            return NotFound();

        return Ok(questionDto);
    }

    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var deleted = await _eventService.DeleteQuestionAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/clone")]
    public async Task<ActionResult<EventDto>> CloneEvent(int id)
    {
        try
        {
            var eventDto = await _eventService.CloneEventAsync(id);
            return Ok(eventDto);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}

