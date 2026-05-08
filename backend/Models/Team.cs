namespace TriviaApp.API.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EventId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
    public ICollection<TeamPoint> TeamPoints { get; set; } = new List<TeamPoint>();
}
