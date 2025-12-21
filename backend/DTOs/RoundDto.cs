namespace TriviaApp.API.DTOs;

public class RoundDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EventId { get; set; }
    public int Order { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}

public class CreateRoundDto
{
    public string Name { get; set; } = string.Empty;
    public int EventId { get; set; }
}

public class UpdateRoundDto
{
    public string Name { get; set; } = string.Empty;
}

