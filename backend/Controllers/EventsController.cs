using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriviaApp.API.DTOs;
using TriviaApp.API.Services;

namespace TriviaApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<List<EventDto>>> GetAllEvents()
    {
        var events = await _eventService.GetAllEventsAsync(UserId);
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var eventDto = await _eventService.GetEventByIdAsync(id, UserId);
        if (eventDto == null)
            return NotFound();

        return Ok(eventDto);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<EventDetailsDto>> GetEventDetails(int id)
    {
        var eventDto = await _eventService.GetEventDetailsAsync(id, UserId);
        if (eventDto == null)
            return NotFound();

        return Ok(eventDto);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent(CreateEventDto createEventDto)
    {
        var eventDto = await _eventService.CreateEventAsync(createEventDto, UserId);
        return CreatedAtAction(nameof(GetEvent), new { id = eventDto.Id }, eventDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EventDto>> UpdateEvent(int id, UpdateEventDto updateEventDto)
    {
        var eventDto = await _eventService.UpdateEventAsync(id, updateEventDto, UserId);
        if (eventDto == null)
            return NotFound();

        return Ok(eventDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var deleted = await _eventService.DeleteEventAsync(id, UserId);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("rounds")]
    public async Task<ActionResult<RoundDto>> CreateRound(CreateRoundDto createRoundDto)
    {
        try
        {
            var roundDto = await _eventService.CreateRoundAsync(createRoundDto, UserId);
            return Ok(roundDto);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpDelete("rounds/{id}")]
    public async Task<IActionResult> DeleteRound(int id)
    {
        var deleted = await _eventService.DeleteRoundAsync(id, UserId);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("categories")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        try
        {
            var categoryDto = await _eventService.CreateCategoryAsync(createCategoryDto, UserId);
            return Ok(categoryDto);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deleted = await _eventService.DeleteCategoryAsync(id, UserId);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("questions")]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionDto createQuestionDto)
    {
        try
        {
            var questionDto = await _eventService.CreateQuestionAsync(createQuestionDto, UserId);
            return Ok(questionDto);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPut("questions/{id}")]
    public async Task<ActionResult<QuestionDto>> UpdateQuestion(int id, UpdateQuestionDto updateQuestionDto)
    {
        var questionDto = await _eventService.UpdateQuestionAsync(id, updateQuestionDto, UserId);
        if (questionDto == null)
            return NotFound();

        return Ok(questionDto);
    }

    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var deleted = await _eventService.DeleteQuestionAsync(id, UserId);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("teams")]
    public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto createTeamDto)
    {
        try
        {
            var teamDto = await _eventService.CreateTeamAsync(createTeamDto, UserId);
            return Ok(teamDto);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("teams/{id}")]
    public async Task<ActionResult<TeamDto>> UpdateTeam(int id, UpdateTeamDto updateTeamDto)
    {
        try
        {
            var teamDto = await _eventService.UpdateTeamAsync(id, updateTeamDto, UserId);
            if (teamDto == null)
                return NotFound();

            return Ok(teamDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("teams/{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var deleted = await _eventService.DeleteTeamAsync(id, UserId);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPut("rounds/{id}/team-points")]
    public async Task<ActionResult<List<TeamPointDto>>> SetRoundTeamPoints(int id, SetRoundTeamPointsDto setRoundTeamPointsDto)
    {
        try
        {
            var teamPoints = await _eventService.SetRoundTeamPointsAsync(id, setRoundTeamPointsDto, UserId);
            return Ok(teamPoints);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/clone")]
    public async Task<ActionResult<EventDto>> CloneEvent(int id)
    {
        try
        {
            var eventDto = await _eventService.CloneEventAsync(id, UserId);
            return Ok(eventDto);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}
