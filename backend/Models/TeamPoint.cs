namespace TriviaApp.API.Models;

public class TeamPoint
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int RoundId { get; set; }
    public int Points { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;
    public Round Round { get; set; } = null!;
}
