namespace TriviaApp.API.DTOs;

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EventId { get; set; }
}

public class CreateTeamDto
{
    public string Name { get; set; } = string.Empty;
    public int EventId { get; set; }
}

public class UpdateTeamDto
{
    public string Name { get; set; } = string.Empty;
}
