namespace TriviaApp.API.DTOs;

public class TeamPointDto
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int RoundId { get; set; }
    public int Points { get; set; }
}

public class SetRoundTeamPointsDto
{
    public List<SetRoundTeamPointItemDto> TeamPoints { get; set; } = new();
}

public class SetRoundTeamPointItemDto
{
    public int TeamId { get; set; }
    public int Points { get; set; }
}
