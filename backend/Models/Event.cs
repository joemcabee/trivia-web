namespace TriviaApp.API.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;

    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}
