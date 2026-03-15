namespace TriviaApp.API.Models;

public class Round
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EventId { get; set; }
    public int Order { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
