namespace TriviaApp.API.DTOs;

public class EventDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<RoundDetailsDto> Rounds { get; set; } = new();
}

public class RoundDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EventId { get; set; }
    public int Order { get; set; }
    public List<CategoryDetailsDto> Categories { get; set; } = new();
}

public class CategoryDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RoundId { get; set; }
    public int Order { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}

